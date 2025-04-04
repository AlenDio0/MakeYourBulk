using System.Collections.Generic;
using Verse;

namespace MakeYourBulk
{
    public class ExposableBackupList : IExposable
    {
        public string listName;
        public List<BulkRecipe> bulkRecipes;

        public ExposableBackupList()
        {
            listName = "ListName";
            bulkRecipes = new List<BulkRecipe>();
        }

        public ExposableBackupList(string listName, List<BulkRecipe> bulkRecipes)
        {
            this.listName = listName;
            this.bulkRecipes = bulkRecipes;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref listName, MYB_Data.ExposableBackupList_ListName);
            Scribe_Collections.Look(ref bulkRecipes, MYB_Data.ExposableBackupList_Recipes, LookMode.Deep);
        }
    }
}
