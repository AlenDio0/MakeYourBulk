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
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(canva.x, canva.y, canva.width, 30f), MYB_Data.AddRecipe_Button);

            Rect searchBarRect = new Rect(canva.x, 40f, 400f, 30f);
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

            Text.Font = GameFont.Small;
            string recipeCount = $"{(recipes.Count >= 10000 ? (float)recipes.Count / 1000f : recipes.Count)}" + (recipes.Count >= 10000 ? "k" : "");
            Rect recipeCountRect = new Rect(searchBarRect.xMax + MYB_Data.DefaultSpace, 40f, 100f, 30f);
            Widgets.Label(recipeCountRect, $"{MYB_Data.RecipesCount_Label}: {recipes.Count}");

            float height = 80f;

            Text.Font = GameFont.Medium;
            Rect scrollRect = new Rect(canva.x, searchBarRect.yMax + MYB_Data.DefaultSpace, canva.width, canva.height - 150f);
            Rect viewRect = new Rect(canva.x, canva.y, scrollRect.width - MYB_Data.DefaultSpace, recipes.Count * height);
            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);

            int index = 0;
            foreach (var recipe in recipes)
            {
                float recipeStart = (index * height);
                Listing_Standard listing = new Listing_Standard();
                listing.Begin(new Rect(canva.x, recipeStart - MYB_Data.DefaultSpace, canva.width - MYB_Data.DefaultSpace, canva.height));
                listing.GapLine();
                listing.End();

                Rect clickableRect = new Rect(canva.x, recipeStart, canva.width - MYB_Data.DefaultSpace, 50f);
                if (Mouse.IsOver(clickableRect))
                {
                    Widgets.DrawHighlight(clickableRect);
                }
                if (Widgets.ButtonInvisible(clickableRect))
                {
                    onSelected?.Invoke(recipe);
                    base.Close();
                }

                Rect thingIconRect = new Rect(canva.x, recipeStart, 48f, 48f);
                Widgets.ThingIcon(thingIconRect, recipe.ProducedThingDef);
                Rect labelRect = new Rect(thingIconRect.xMax + MYB_Data.DefaultSpace * 2f, recipeStart, canva.xMax - thingIconRect.xMax + MYB_Data.DefaultSpace * 2f, 50f);
                Widgets.Label(labelRect, recipe.LabelCap);
                index++;
            }

            Widgets.EndScrollView();
        }
    }
}