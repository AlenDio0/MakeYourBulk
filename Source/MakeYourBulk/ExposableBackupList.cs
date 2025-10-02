using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MakeYourBulk
{
    public class ExposableBackupList : IExposable
    {
        public string _ListName;
        public List<BulkRecipe> _BulkRecipes;

        public ExposableBackupList()
        {
        }

        public ExposableBackupList(string listName, List<BulkRecipe> bulkRecipes)
        {
            _ListName = listName;
            _BulkRecipes = bulkRecipes.ToList();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _ListName, MYB_Data.ExposableBackupList_ListName);
            Scribe_Collections.Look(ref _BulkRecipes, MYB_Data.ExposableBackupList_Recipes, LookMode.Deep);
        }
    }
}
