using Force.DeepCloner;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MakeYourBulk
{
    public class MakeYourBulkMod : Mod
    {
        public static MakeYourBulkSettings s_Settings;
        public static ModContentPack s_ModContent;

        public MakeYourBulkMod(ModContentPack content)
            : base(content)
        {
            s_Settings = GetSettings<MakeYourBulkSettings>();
            s_ModContent = content;

            LongEventHandler.ExecuteWhenFinished(BulkRecipeGenerator.LoadBulkRecipeDefs);
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            if (s_Settings == null)
            {
                MYB_Log.Error("ModSettings is null");
                return;
            }

            s_Settings.DoWindowContents(inRect);
            base.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();

            BulkRecipeGenerator.LoadBulkRecipeDefs();

            foreach (BulkRecipe bulkRecipe in s_Settings.AllBulkRecipe)
            {
                if (bulkRecipe?.BaseRecipeDef == null)
                    continue;

                if (BulkRecipeGenerator.LoadedBulkRecipeDefs.TryGetValue(bulkRecipe.DefName, out RecipeDef recipeDef))
                {
                    BulkRecipeGenerator.SetBulkWorkAmount(recipeDef, bulkRecipe.BaseRecipeDef, bulkRecipe.TotalWorkAmountFactor);
                    BulkRecipeGenerator.SetBulkIngredients(recipeDef, bulkRecipe.BaseRecipeDef, bulkRecipe.TotalCostFactor);
                }
            }
        }

        public override string SettingsCategory() => MYB_Data.SpacedModName;
    }

    public static class BulkRecipeGenerator
    {
        private readonly static Dictionary<string, RecipeDef> m_BulkRecipeDefs = new Dictionary<string, RecipeDef>();
        public static Dictionary<string, RecipeDef> LoadedBulkRecipeDefs => m_BulkRecipeDefs;

        public static void LoadBulkRecipeDefs()
        {
            var settings = MakeYourBulkMod.s_Settings;

            foreach (BulkRecipe bulkRecipe in settings.AllBulkRecipe)
            {
                if (bulkRecipe?.BaseRecipeDef == null)
                    continue;

                if (DefDatabase<RecipeDef>.GetNamedSilentFail(bulkRecipe.DefName) == null)
                {
                    RecipeDef bulkRecipeDef = CreateBulkRecipeDef(bulkRecipe, settings.AddUnfinishedThing, settings.SameQuality);
                    if (bulkRecipeDef == null)
                        continue;

                    AddIntoDefDatabase(bulkRecipeDef);
                    AddIntoMissingRecipeUsers(bulkRecipeDef, bulkRecipe.BaseRecipeDef);

                    m_BulkRecipeDefs.Add(bulkRecipe.DefName, bulkRecipeDef);
                }
            }
            DefDatabase<RecipeDef>.ResolveAllReferences();
        }

        private static void AddIntoDefDatabase(RecipeDef recipe)
        {
            MYB_Log.Trace($"Adding '{recipe.defName}' into DefDatabase<RecipeDef>");
            DefDatabase<RecipeDef>.Add(recipe);
        }

        private static void AddIntoMissingRecipeUsers(RecipeDef recipe, RecipeDef baseRecipe)
        {
            var recipeUsers = DefDatabase<ThingDef>.AllDefs
                .Where(recipeUser => recipeUser.recipes != null && recipeUser.recipes.Contains(baseRecipe));

            if (!recipeUsers.EnumerableNullOrEmpty())
            {
                string users = "";
                foreach (string user in recipeUsers.Select(user => user.LabelCap))
                    users += $"{user}, ";
                users = users.Substring(0, users.Length - 2);

                MYB_Log.Trace($"Adding '{recipe.defName}' into: {users}");
                foreach (ThingDef recipeUser in recipeUsers)
                    recipeUser.recipes.Add(recipe);
            }
        }

        private static RecipeDef CreateBulkRecipeDef(BulkRecipe bulkRecipe, bool addUnfinishedThing, bool sameQuality)
        {
            if (bulkRecipe.BaseRecipeDef == null)
            {
                MYB_Log.Error("BaseRecipeDef is null");
                return null;
            }
            if (bulkRecipe.ProductThingDef == null)
            {
                MYB_Log.Error("ProductThingDef is null");
                return null;
            }
            RecipeDef bulkRecipeDef = bulkRecipe.BaseRecipeDef.ShallowClone();
            if (bulkRecipeDef == null)
            {
                MYB_Log.Error($"Cloning BaseRecipeDef {bulkRecipe.BaseRecipeDef.defName} failed");
                return null;
            }

            bulkRecipeDef.ClearCachedData();
            bulkRecipeDef.regenerateOnDifficultyChange = false;
            bulkRecipeDef.defName = bulkRecipe.DefName;
            bulkRecipeDef.label = bulkRecipe.RealLabel;
            bulkRecipeDef.modContentPack = MakeYourBulkMod.s_ModContent;
            bulkRecipeDef.description = $"{bulkRecipe.RealLabel}\n\n[{MYB_Data.SpacedModName} Mod]";
            bulkRecipeDef.jobString = $"{MYB_Data.JobPrefix} {bulkRecipe.ProductThingDef.label} x{bulkRecipe.TotalProduct}";

            bulkRecipeDef.products = CreateRecipeProducts(bulkRecipe.ProductThingDef, bulkRecipe.TotalProduct, sameQuality).ToList();

            SetBulkWorkAmount(bulkRecipeDef, bulkRecipe.BaseRecipeDef, bulkRecipe.TotalWorkAmountFactor);

            bulkRecipeDef.adjustedCount = bulkRecipe.TotalProduct;
            SetBulkIngredients(bulkRecipeDef, bulkRecipe.BaseRecipeDef, bulkRecipe.TotalCostFactor);

            bulkRecipeDef.descriptionHyperlinks = bulkRecipe.BaseRecipeDef.descriptionHyperlinks?.ToList();
            if (bulkRecipeDef.descriptionHyperlinks == null)
                bulkRecipeDef.descriptionHyperlinks = new List<DefHyperlink>();
            bulkRecipeDef.descriptionHyperlinks.Add(new DefHyperlink(bulkRecipe.BaseRecipeDef));

            if (!bulkRecipeDef.UsesUnfinishedThing && addUnfinishedThing)
                bulkRecipeDef.unfinishedThingDef = ThingDef.Named(MYB_Data.UnfinishedBulkDefName);

            return bulkRecipeDef;
        }

        private static IEnumerable<ThingDefCountClass> CreateRecipeProducts(ThingDef product, int count, bool sameQuality)
        {
            if (!sameQuality && product.HasComp(typeof(CompQuality)))
                for (int i = 0; i < count; i++)
                    yield return new ThingDefCountClass(product, 1);
            else
                yield return new ThingDefCountClass(product, count);
        }

        public static void SetBulkWorkAmount(RecipeDef bulkRecipe, RecipeDef baseRecipe, float factor)
        {
            float work = baseRecipe.WorkAmountForStuff(null) * factor;
            bulkRecipe.workAmount = work;
            bulkRecipe.smeltingWorkAmount = work;
        }

        public static void SetBulkIngredients(RecipeDef bulkRecipe, RecipeDef baseRecipe, float factor)
        {
            bulkRecipe.ingredients = new List<IngredientCount>();
            foreach (IngredientCount ingredient in baseRecipe.ingredients)
            {
                IngredientCount newIngredient = new IngredientCount();

                float newCost = ingredient.GetBaseCount() * factor;
                newIngredient.SetBaseCount(newCost);

                newIngredient.filter.CopyAllowancesFrom(ingredient.filter);

                bulkRecipe.ingredients.Add(newIngredient);
            }
        }
    }
}
