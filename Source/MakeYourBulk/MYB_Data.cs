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

        public static string DefaultListName => "DefaultListName".Translate();

        public static float DefaultSpace => 20f;
        public static Vector2 ThirdSize(float width) => new Vector2((width - (DefaultSpace * 3f)) / 3f, 30f);

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

        public static bool Settings_DefaultVerboseLogging => true;
        public static bool Settings_DefaultAddUnfishedThing => false;
        public static bool Settings_DefaultSameQuality => false;

        public static string ExposableBackupList_ListName => "ListName";
        public static string ExposableBackupList_Recipes => "Recipes";

        public static string BulkRecipe_RecipeDefName => "recipeDefName";
        public static string BulkRecipe_Products => "products";
        public static string BulkRecipe_WorkAmount => "workAmount";
        public static string BulkRecipe_Cost => "cost";
        public static string BulkRecipe_DefaultRecipeDefName => "UnknownRecipe";
        public static int BulkRecipe_DefaultProducts => 5;
        public static float BulkRecipe_DefaultWorkAmount => 1;
        public static float BulkRecipe_DefaultCost => 1;
    }
}
