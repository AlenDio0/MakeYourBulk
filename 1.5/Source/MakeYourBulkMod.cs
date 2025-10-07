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

        public override string SettingsCategory() => MYB_Data.SpacedModName;
    }

    public static class BulkRecipeGenerator
    {
        public static void LoadBulkRecipeDefs()
        {
            var settings = LoadedModManager.GetMod<MakeYourBulkMod>().GetSettings<MakeYourBulkSettings>();

            foreach (BulkRecipe recipe in settings.AllBulkRecipe)
            {
                if (recipe?.GetBaseRecipe() == null)
                    continue;

                if (DefDatabase<RecipeDef>.GetNamedSilentFail(recipe.DefName) == null)
                {
                    RecipeDef recipeDef = recipe.GetBulkRecipeDef(settings.AddUnfinishedThing, settings.SameQuality);
                    if (recipeDef == null)
                        continue;

                    AddRecipeIntoDefDatabase(recipeDef);
                    AddRecipeIntoMissingRecipeUsers(recipeDef, recipe.GetBaseRecipe());
                }
            }
            DefDatabase<RecipeDef>.ResolveAllReferences();
        }

        private static void AddRecipeIntoDefDatabase(RecipeDef recipe)
        {
            MYB_Log.Trace($"Adding '{recipe.defName}' into DefDatabase<RecipeDef>");
            DefDatabase<RecipeDef>.Add(recipe);
        }

        private static void AddRecipeIntoMissingRecipeUsers(RecipeDef recipe, RecipeDef baseRecipe)
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
    }
}
