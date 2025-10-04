using Force.DeepCloner;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MakeYourBulk
{
    [Serializable]
    public class BulkRecipe : IExposable, IRenameable
    {
        private string m_BaseDefName;
        public string BaseDefName => m_BaseDefName;
        private RecipeDef m_CachedBaseRecipe = null;

        private string m_CustomLabel = null;

        private RecipeDef m_CachedBulkRecipe = null;
        private BulkProperties m_LastProperties = null;

        public BulkProperties _Properties;
        public int Product => _Properties._Product;
        public float WorkAmount => _Properties._WorkAmount;
        public float Cost => _Properties._Cost;

        public BulkRecipe()
        {
            _Properties = new BulkProperties();
        }

        public BulkRecipe(RecipeDef baseRecipe, BulkProperties properties)
        {
            m_CachedBaseRecipe = baseRecipe;
            m_BaseDefName = baseRecipe.defName;

            _Properties = properties.ShallowClone();
        }

        public static bool CanBeBulk(RecipeDef recipeDef) =>
            recipeDef.ProducedThingDef != null &&
            recipeDef.products.Count == 1 &&
            !recipeDef.IsSurgery &&
            !recipeDef.mechanitorOnlyRecipe;

        public RecipeDef GetBaseRecipe()
        {
            if (m_CachedBaseRecipe == null)
            {
                if (BaseDefName.NullOrEmpty())
                {
                    MYB_Log.Warn("BulkRecipe BaseDefName null or empty");
                    return null;
                }

                m_CachedBaseRecipe = DefDatabase<RecipeDef>.GetNamedSilentFail(BaseDefName);
                if (m_CachedBaseRecipe == null)
                {
                    MYB_Log.Warn($"'{BaseDefName}' not found in DefDatabase<RecipeDef>");
                    return null;
                }
            }

            return m_CachedBaseRecipe;
        }

        public ThingDef ProductThingDef => GetBaseRecipe().ProducedThingDef;

        public int RealProducts => Product * GetBaseRecipe().products.First().count;
        public string WorkAmountPercent => WorkAmount.ToStringPercent();
        public string CostPercent => Cost.ToStringPercent();

        public string RenamableLabel
        {
            get => m_CustomLabel ?? BaseLabel;
            set => m_CustomLabel = value;
        }
        public string BaseLabel => GetBaseRecipe().LabelCap;
        public string InspectLabel => RenamableLabel;
        public string RealLabel => $"{RenamableLabel} x{RealProducts}";

        public string DefName => $"MYB_{BaseDefName}_x{RealProducts}";

        public RecipeDef GetBulkRecipeDef(bool addUnfinishedThing, bool sameQuality)
        {
            bool properties = m_LastProperties != _Properties;
            bool label = m_CachedBulkRecipe?.label != RealLabel;

            if (m_CachedBaseRecipe == null || properties || label)
            {
                m_LastProperties = _Properties.ShallowClone();

                m_CachedBulkRecipe = CreateBulkRecipeDef(addUnfinishedThing, sameQuality);
            }

            return m_CachedBulkRecipe;
        }

        private RecipeDef CreateBulkRecipeDef(bool addUnfinishedThing, bool sameQuality)
        {
            if (ProductThingDef == null)
            {
                MYB_Log.Error("BaseRecipeDef or ProductThingDef is null");
                return null;
            }
            RecipeDef bulkRecipeDef = GetBaseRecipe().ShallowClone();
            if (bulkRecipeDef == null)
            {
                MYB_Log.Error("Cloning BaseRecipeDef failed");
                return null;
            }

            bulkRecipeDef.ClearCachedData();
            bulkRecipeDef.regenerateOnDifficultyChange = false;
            bulkRecipeDef.defName = DefName;
            bulkRecipeDef.label = RealLabel;
            bulkRecipeDef.modContentPack = MakeYourBulkMod.s_ModContent;
            bulkRecipeDef.description = $"{RealLabel}\n\n[{MYB_Data.SpacedModName} Mod]";
            bulkRecipeDef.jobString = $"{MYB_Data.JobPrefix} {ProductThingDef.label} x{RealProducts}";

            bulkRecipeDef.products = CreateRecipeProducts(ProductThingDef, RealProducts, sameQuality).ToList();
            bulkRecipeDef.ingredients = CreateIngredients(GetBaseRecipe(), Product * Cost).ToList();

            bulkRecipeDef.descriptionHyperlinks = GetBaseRecipe().descriptionHyperlinks?.ToList();
            if (bulkRecipeDef.descriptionHyperlinks == null)
                bulkRecipeDef.descriptionHyperlinks = new List<DefHyperlink>();
            bulkRecipeDef.descriptionHyperlinks.Add(new DefHyperlink(GetBaseRecipe()));

            float work = bulkRecipeDef.WorkAmountForStuff(null) * WorkAmount * Product;
            bulkRecipeDef.workAmount = work;
            bulkRecipeDef.smeltingWorkAmount = work;

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

        private static IEnumerable<IngredientCount> CreateIngredients(RecipeDef baseRecipe, float factor)
        {
            foreach (IngredientCount ingredient in baseRecipe.ingredients)
            {
                IngredientCount newIngredient = new IngredientCount
                {
                    filter = new ThingFilter()
                };
                newIngredient.filter.CopyAllowancesFrom(ingredient.filter);

                float newCost = ingredient.GetBaseCount() * factor;
                newIngredient.SetBaseCount(newCost);

                yield return newIngredient;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref m_CustomLabel, MYB_Data.BulkRecipe_CustomLabel, null);
            Scribe_Values.Look(ref m_BaseDefName, MYB_Data.BulkRecipe_RecipeDefName, MYB_Data.BulkRecipe_DefaultRecipeDefName);

            Scribe_Values.Look(ref _Properties._Product, MYB_Data.BulkProperties_Products, MYB_Data.BulkProperties_DefaultProducts);
            Scribe_Values.Look(ref _Properties._WorkAmount, MYB_Data.BulkProperties_WorkAmount, MYB_Data.BulkProperties_DefaultWorkAmount);
            Scribe_Values.Look(ref _Properties._Cost, MYB_Data.BulkProperties_Cost, MYB_Data.BulkProperties_DefaultCost);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BaseDefName, _Properties.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BulkRecipe other))
            {
                return false;
            }

            return GetHashCode() == other.GetHashCode();
        }
    }

    public class Dialog_RenameBulkRecipe : Dialog_Rename<BulkRecipe>
    {
        public Dialog_RenameBulkRecipe(BulkRecipe renaming)
            : base(renaming)
        {
        }
    }
}
