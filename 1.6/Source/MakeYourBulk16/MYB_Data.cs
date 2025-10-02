using UnityEngine;
using Verse;

namespace MakeYourBulk
{
    [StaticConstructorOnStartup]
    public static class MYB_Data
    {
        public static string ModName => "MakeYourBulk";
        public static string SpacedModName => "Make Your Bulk";

        public static string DefaultUnfinishedThing => "UnfinishedComponent";

        public static float GapX => 20f;
        public static float GapY => 5f;
        public static Rect LeftThird(Rect rect, float part = 0.99f) => new Rect(rect.x, rect.y, rect.width / 3f, rect.height).LeftPart(part).RightPart(part);
        public static Rect MiddleThird(Rect rect, float part = 0.99f) => new Rect(rect.x + rect.width / 3f, rect.y, rect.width / 3f, rect.height).LeftPart(part).RightPart(part);
        public static Rect RightThird(Rect rect, float part = 0.99f) => new Rect(rect.x + rect.width / 1.5f, rect.y, rect.width / 3f, rect.height).LeftPart(part).RightPart(part);


        public static string VerboseLogging_Label => "VerboseLogging_Label".Translate();
        public static string AddBulkUnfinishedThing_Label => "AddBulkUnfinishedThing_Label".Translate();
        public static string SameQuality_Label => "SameQuality_Label".Translate();

        public static string VerboseLogging_Tooltip => "VerboseLogging_Tooltip".Translate();
        public static string AddBulkUnfinishedThing_Tooltip => "AddBulkUnfinishedThing_Tooltip".Translate();
        public static string SameQuality_Tooltip => "SameQuality_Tooltip".Translate();

        public static string AddRecipe_Button => "AddRecipe_Button".Translate();
        public static string RemoveRecipe_Button => "RemoveRecipe_Button".Translate();
        public static string SaveLoad_Button => "SaveLoad_Button".Translate();
        public static string Reset_Button => "Reset_Button".Translate();

        public static string RecipesCount_Label => "RecipesCount_Label".Translate();

        public static string Save_Option => "Save_Option".Translate();
        public static string Load_Option => "Load_Option".Translate();
        public static string SaveList_Button => "SaveList_Button".Translate();
        public static string OverwriteList_Button => "OverwriteList_Button".Translate();
        public static string LoadList_Button => "LoadList_Button".Translate();
        public static string AddList_Button => "AddList_Button".Translate();
        public static string LoadListEmpty_Label => "LoadListEmpty_Label".Translate();
        public static string EmptyList_Message => "EmptyList_Message".Translate();
        public static string LoadListDialog_Message(string listName) => "LoadListDialog_Message".Translate(listName);
        public static string LoadListDialog_Title(string listName) => "LoadListDialog_Title".Translate(listName);
        public static string DeleteListDialog_Message(string listName) => "DeleteListDialog_Message".Translate(listName);
        public static string DeleteListDialog_Title(string listName) => "DeleteListDialog_Title".Translate(listName);

        public static string ResetDialog_Message => "ResetDialog_Message".Translate();

        public static string SearchBox_Label => "SearchBox_Label".Translate();

        public static string RecipePrefix => "RecipePrefix".Translate();
        public static string JobPrefix => "JobPrefix".Translate();

        public static string RecipeProducts_Label => "RecipeProducts_Label".Translate();
        public static string RecipeWorkAmount_Label => "RecipeWorkAmount_Label".Translate();
        public static string RecipeCost_Label => "RecipeCost_Label".Translate();

        public static string Attention_Label => "Attention_Label".Translate();

        public static string Confirm_Button => "Confirm_Button".Translate();
        public static string Cancel_Button => "Cancel_Button".Translate();

        public static string Settings_BackupList => "BackupLists";
        public static string Settings_Recipes => "MYB_Recipes";
        public static string Settings_VerboseLogging => "VerboseLogging";
        public static string Settings_AddUnfinishedThing => "AddUnfinishedThing";
        public static string Settings_SameQuality => "SameQuality";
        public static string Settings_Properties => "Properties";
        public static string Settings_HeightLevel => "HeightLevel";

        public static bool Settings_DefaultVerboseLogging => true;
        public static bool Settings_DefaultAddUnfishedThing => false;
        public static bool Settings_DefaultSameQuality => true;
        public static float Settings_DefaultHeightLevel => 3f;

        public static string ExposableBackupList_ListName => "ListName";
        public static string ExposableBackupList_Recipes => "Recipes";

        public static string BulkRecipe_CustomLabel => "CustomLabel";
        public static string BulkRecipe_RecipeDefName => "recipeDefName";

        public static string BulkRecipe_DefaultRecipeDefName => "UnknownRecipe";

        public static string BulkProperties_Products => "products";
        public static string BulkProperties_WorkAmount => "workAmount";
        public static string BulkProperties_Cost => "cost";

        public static int BulkProperties_DefaultProducts => 5;
        public static float BulkProperties_DefaultWorkAmount => 1;
        public static float BulkProperties_DefaultCost => 1;

    }
}
