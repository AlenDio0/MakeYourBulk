using System;
using System.Linq;
using Verse;

namespace MakeYourBulk
{
    [Serializable]
    public class BulkRecipe : IExposable, IRenameable
    {
        private string m_BaseRecipeDefName;
        public string BaseRecipeDefName => m_BaseRecipeDefName;

        private RecipeDef m_CachedBaseRecipe = null;
        public RecipeDef BaseRecipeDef
        {
            get
            {
                if (m_CachedBaseRecipe == null)
                {
                    if (BaseRecipeDefName.NullOrEmpty())
                    {
                        MYB_Log.Warn("BulkRecipe BaseDefName null or empty");
                        return null;
                    }

                    m_CachedBaseRecipe = DefDatabase<RecipeDef>.GetNamedSilentFail(BaseRecipeDefName);
                    if (m_CachedBaseRecipe == null)
                    {
                        MYB_Log.Warn($"'{BaseRecipeDefName}' not found in DefDatabase<RecipeDef>");
                        return null;
                    }
                }

                return m_CachedBaseRecipe;
            }
        }

        public ThingDef ProductThingDef => BaseRecipeDef.ProducedThingDef;

        private string m_CustomLabel = null;

        public int _Product;
        public float _WorkAmount;
        public float _Cost;

        public string _ProductBuffer;

        public int TotalProduct => _Product * BaseRecipeDef.products.First().count;
        public float TotalWorkAmountFactor => _Product * _WorkAmount;
        public float TotalCostFactor => _Product * _Cost;

        public string RenamableLabel
        {
            get => m_CustomLabel ?? BaseLabel;
            set => m_CustomLabel = value;
        }
        public string BaseLabel => BaseRecipeDef.LabelCap;
        public string InspectLabel => RenamableLabel;
        public string RealLabel => $"{RenamableLabel} x{TotalProduct}";

        public string DefName => $"MYB_{BaseRecipeDefName}_x{TotalProduct}";

        public BulkRecipe()
        {
            _Product = 5;
            _WorkAmount = 1f;
            _Cost = 1f;
        }

        public BulkRecipe(RecipeDef baseRecipe, int product, float workAmount, float cost)
        {
            m_CachedBaseRecipe = baseRecipe;
            m_BaseRecipeDefName = baseRecipe.defName;

            _Product = product;
            _WorkAmount = workAmount;
            _Cost = cost;
        }

        public static bool CanBeBulk(RecipeDef recipeDef) =>
            recipeDef.ProducedThingDef != null &&
            recipeDef.products.Count == 1 &&
            !recipeDef.IsSurgery &&
            !recipeDef.mechanitorOnlyRecipe;

        public void ExposeData()
        {
            Scribe_Values.Look(ref m_CustomLabel, MYB_Data.BulkRecipe_CustomLabel, null);
            Scribe_Values.Look(ref m_BaseRecipeDefName, MYB_Data.BulkRecipe_RecipeDefName, MYB_Data.BulkRecipe_DefaultRecipeDefName);

            Scribe_Values.Look(ref _Product, MYB_Data.BulkProperties_Products, MYB_Data.BulkProperties_DefaultProducts);
            Scribe_Values.Look(ref _WorkAmount, MYB_Data.BulkProperties_WorkAmount, MYB_Data.BulkProperties_DefaultWorkAmount);
            Scribe_Values.Look(ref _Cost, MYB_Data.BulkProperties_Cost, MYB_Data.BulkProperties_DefaultCost);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BaseRecipeDefName, _Product, _WorkAmount, _Cost);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BulkRecipe other))
                return false;

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
