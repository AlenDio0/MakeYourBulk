using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
namespace MakeYourBulk
{
    public class Dialog_AddRecipe : Window
    {
        private readonly Action<RecipeDef> onSelected;

        string searchBoxBuffer = "";

        private Vector2 scrollPosition = Vector2.zero;

        public Dialog_AddRecipe(Action<RecipeDef> onSelected)
        {
            forcePause = true;
            doCloseButton = true;
            closeOnAccept = true;
            closeOnCancel = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            this.onSelected = onSelected;
        }

        public override Vector2 InitialSize => new Vector2(600f, 800f);

        public override void DoWindowContents(Rect canva)
        {
            GameFont defaultFont = Text.Font;

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(canva);

            Text.Font = GameFont.Medium;
            Rect currentRow = listing.GetRect(Text.LineHeight + 4f);

            Rect titleRect = new Rect(currentRow.x, currentRow.y, currentRow.width, 30f);
            Widgets.Label(titleRect, MYB_Data.AddRecipe_Button);

            Text.Font = GameFont.Small;
            currentRow = listing.GetRect(30f + MYB_Data.DefaultSpace);

            Rect searchBarRect = new Rect(currentRow.x, currentRow.y, 400f, 30f);
            searchBoxBuffer = Widgets.TextField(searchBarRect, searchBoxBuffer);

            List<RecipeDef> recipes = new List<RecipeDef>();
            foreach (RecipeDef recipeDef in DefDatabase<RecipeDef>.AllDefs.Where(r => BulkRecipe.CanBeBulk(r)))
            {
                if (!recipeDef.label.ToLower().Contains(searchBoxBuffer.ToLower()) && !searchBoxBuffer.NullOrEmpty())
                {
                    continue;
                }

                recipes.Add(recipeDef);
            }

            string recipeCount = (recipes.Count >= 10000 ? recipes.Count / 1000f : recipes.Count) + (recipes.Count >= 10000 ? "k" : "");
            Rect recipeCountRect = new Rect(searchBarRect.xMax + MYB_Data.DefaultSpace, 40f, 100f, 30f);
            Widgets.Label(recipeCountRect, $"{MYB_Data.RecipesCount_Label}: {recipes.Count}");

            listing.GapLine();
            currentRow = listing.GetRect(canva.height - listing.CurHeight - 100f);

            float height = 50f;
            Rect scrollRect = new Rect(currentRow.x, currentRow.y, currentRow.width - MYB_Data.DefaultSpace, recipes.Count * (height + 12f));
            Widgets.BeginScrollView(currentRow, ref scrollPosition, scrollRect);

            Listing_Standard scrollListing = new Listing_Standard();
            scrollListing.Begin(scrollRect);

            foreach (var recipe in recipes)
            {
                currentRow = scrollListing.GetRect(height);

                Rect clickableRect = new Rect(currentRow.x, currentRow.y, currentRow.width - MYB_Data.DefaultSpace, 50f);
                if (Mouse.IsOver(clickableRect))
                {
                    Widgets.DrawHighlight(clickableRect);
                }
                if (Widgets.ButtonInvisible(clickableRect))
                {
                    onSelected?.Invoke(recipe);
                    base.Close();
                }

                float iconSize = 48f;
                Rect thingIconRect = new Rect(currentRow.x, currentRow.y, iconSize, iconSize);
                ThingDef thingDef = recipe.ProducedThingDef;
                Widgets.ThingIcon(thingIconRect, thingDef);

                Rect labelRect = new Rect(thingIconRect.xMax + MYB_Data.DefaultSpace * 2f, currentRow.y, currentRow.width, 50f);
                Widgets.Label(labelRect, recipe.LabelCap);

                scrollListing.GapLine();
            }
            Widgets.EndScrollView();

            scrollListing.End();

            listing.GapLine();
            listing.End();

            Text.Font = defaultFont;
        }
    }
}