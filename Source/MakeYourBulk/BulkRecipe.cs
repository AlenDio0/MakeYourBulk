using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MakeYourBulk
{
    [Serializable]
    public class BulkRecipe : IExposable
    {
        public RecipeDef recipeDef;
        private string recipeDefName;
        public string RecipeDefName
        {
            get
            {
                return recipeDefName;
            }
        }

        public BulkProp prop;

        public BulkRecipe()
        {
            prop = new BulkProp();
        }

        public BulkRecipe(RecipeDef recipeDef, BulkProp prop = null)
        {
            this.recipeDef = recipeDef;
            recipeDefName = recipeDef.defName;

            this.prop = prop;
            if (prop == null)
            {
                this.prop = new BulkProp();
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BulkRecipe other)) return false;
            return (DefName != null && DefName == other.DefName) && (prop != null && prop.Equals(other.prop));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DefName, prop);
        }

        public static bool CompareLists(List<BulkRecipe> list1, List<BulkRecipe> list2)
        {
            if (list1.NullOrEmpty() || list2.NullOrEmpty())
            {
                return false;
            }
            if (list1.Count != list2.Count)
            {
                return false;
            }

            var sortedList1 = list1.OrderBy(x => x.recipeDef.defName).ToList();
            var sortedList2 = list2.OrderBy(x => x.recipeDef.defName).ToList();

            for (int i = 0; i < sortedList1.Count; i++)
            {
                if (!sortedList1[i].Equals(sortedList2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CanBeBulk(RecipeDef recipeDef)
        {
            return !recipeDef.IsSurgery && !recipeDef.mechanitorOnlyRecipe && recipeDef.ProducedThingDef != null && recipeDef.products.Count == 1;
        }

        public int RealProducts => prop.products * recipeDef.products.First().count;

        public string DefName
        {
            get
            {
                if (recipeDef == null || recipeDef.ProducedThingDef == null)
                {
                    return null;
                }

                return $"MYB_Make_{recipeDef.ProducedThingDef.label}_x{RealProducts}";
            }
        }

        public string Label
        {
            get
            {
                if (recipeDef == null || recipeDef.ProducedThingDef == null)
                {
                    return null;
                }

                return $"{MYB_Data.RecipePrefix} {recipeDef.ProducedThingDef.label} x{RealProducts}";
            }
        }

        public void AdjustRecipeDef()
        {
            if (RecipeDefName.NullOrEmpty())
            {
                return;
            }

            recipeDef = DefDatabase<RecipeDef>.GetNamedSilentFail(RecipeDefName);
            if (recipeDef == null)
            {
                MYB_Log.Warn($"'{RecipeDefName}' not found in DefDatBase<RecipeDef>");
            }
        }

        public RecipeDef CreateBulkRecipeDef(bool addUnfinishedThing, bool sameQuality)
        {
            AdjustRecipeDef();
            if (recipeDef == null)
            {
                MYB_Log.Error("RecipeDef is null");
                return null;
            }
            if (recipeDef.ProducedThingDef == null)
            {
                MYB_Log.Error("ProducedThingDef is null");
                return null;
            }

            ThingDefCountClass producedThing = recipeDef.products.First();
            RecipeMakerProperties productRecipeMaker = producedThing.thingDef.recipeMaker;
            float workToMake = producedThing.thingDef.statBases.FirstOrDefault(x => x.stat == StatDefOf.WorkToMake)?.value ?? -1f;
            if (workToMake == -1f)
            {
                workToMake = recipeDef.WorkAmountForStuff(producedThing.thingDef);
            }

            RecipeDef bulkRecipeDef = CopyFromRecipe(recipeDef);
            bulkRecipeDef.defName = DefName;
            bulkRecipeDef.label = Label;
            bulkRecipeDef.description = $"{Label}\n\n[{MYB_Data.SpacedModName} Mod]";
            bulkRecipeDef.jobString = $"{MYB_Data.JobPrefix} {producedThing.thingDef.label} x{prop.products}";
            bulkRecipeDef.workAmount = workToMake * prop.workAmount * prop.products;
            bulkRecipeDef.smeltingWorkAmount = workToMake * prop.workAmount * prop.products;

            bulkRecipeDef.products = new List<ThingDefCountClass>();
            if (!sameQuality && producedThing.thingDef.HasComp(typeof(CompQuality)))
            {
                for (int i = 0; i < RealProducts; i++)
                {
                    bulkRecipeDef.products.Add(new ThingDefCountClass(producedThing.thingDef, 1));
                }
            }
            else
            {
                bulkRecipeDef.products.Add(new ThingDefCountClass(producedThing.thingDef, RealProducts));
            }

            if (recipeDef.ingredients != null)
            {
                bulkRecipeDef.ingredients = new List<IngredientCount>();

                foreach (IngredientCount ingredient in recipeDef.ingredients)
                {
                    IngredientCount newIngredient = new IngredientCount();

                    newIngredient.filter = new ThingFilter();
                    newIngredient.filter.CopyAllowancesFrom(ingredient.filter);

                    float baseCost = ingredient.GetBaseCount();
                    float newCost = baseCost * prop.products * prop.cost;

                    newIngredient.SetBaseCount(newCost);

                    bulkRecipeDef.ingredients.Add(newIngredient);
                }
            }

            Traverse.Create(bulkRecipeDef).Field("ingredientValueGetterClass").SetValue(Traverse.Create(recipeDef).Field("ingredientValueGetterClass").GetValue());

            if (!bulkRecipeDef.UsesUnfinishedThing && addUnfinishedThing)
            {
                bulkRecipeDef.unfinishedThingDef = ThingDef.Named(MYB_Data.DefaultUnfinishedThing);
            }

            return bulkRecipeDef;
        }

        private RecipeDef CopyFromRecipe(RecipeDef copy)
        {
            return new RecipeDef
            {
                displayPriority = copy.displayPriority,
                modContentPack = copy.modContentPack,
                useIngredientsForColor = copy.useIngredientsForColor,
                allowMixingIngredients = copy.allowMixingIngredients,
                fixedIngredientFilter = copy.fixedIngredientFilter,
                defaultIngredientFilter = copy.defaultIngredientFilter ?? copy.ProducedThingDef.recipeMaker?.defaultIngredientFilter,
                workSpeedStat = copy.workSpeedStat ?? copy.ProducedThingDef.recipeMaker?.workSpeedStat,
                soundWorking = copy.soundWorking ?? copy.ProducedThingDef.recipeMaker?.soundWorking,
                effectWorking = copy.effectWorking ?? copy.ProducedThingDef.recipeMaker?.effectWorking,
                unfinishedThingDef = copy.unfinishedThingDef ?? copy.ProducedThingDef.recipeMaker?.unfinishedThingDef,
                requiredGiverWorkType = copy.requiredGiverWorkType ?? copy.ProducedThingDef.recipeMaker?.requiredGiverWorkType,
                workSkill = copy.workSkill ?? copy.ProducedThingDef.recipeMaker?.workSkill,
                skillRequirements = copy.skillRequirements?.ToList() ?? copy.ProducedThingDef.recipeMaker?.skillRequirements?.ToList(),
                researchPrerequisite = copy.researchPrerequisite ?? copy.ProducedThingDef.recipeMaker?.researchPrerequisite,
                researchPrerequisites = copy.researchPrerequisites?.ToList() ?? copy.ProducedThingDef.recipeMaker?.researchPrerequisites?.ToList(),
                recipeUsers = copy.AllRecipeUsers.ToList(),
                factionPrerequisiteTags = copy.factionPrerequisiteTags?.ToList() ?? copy.ProducedThingDef.recipeMaker?.factionPrerequisiteTags?.ToList(),
                memePrerequisitesAny = copy?.memePrerequisitesAny?.ToList() ?? copy.ProducedThingDef.recipeMaker?.memePrerequisitesAny?.ToList(),
            };
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref recipeDefName, MYB_Data.BulkRecipe_RecipeDefName, MYB_Data.BulkRecipe_DefaultRecipeDefName);
            Scribe_Values.Look(ref prop.products, MYB_Data.BulkRecipe_Products, MYB_Data.BulkRecipe_DefaultProducts);
            Scribe_Values.Look(ref prop.workAmount, MYB_Data.BulkRecipe_WorkAmount, MYB_Data.BulkRecipe_DefaultWorkAmount);
            Scribe_Values.Look(ref prop.cost, MYB_Data.BulkRecipe_Cost, MYB_Data.BulkRecipe_DefaultCost);
        }
    }
}
