using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MakeYourBulk
{
    public class Dialog_AddRecipe : Window
    {
        private readonly List<BulkRecipe> m_BulkRecipes;
        private readonly BulkProperties m_Properties;

        private List<RecipeDef> m_CachedShowableRecipes = null;
        private string m_LastSearchboxBuffer;
        private int m_LastDefsCount = 0;

        private string m_SearchboxBuffer = "";

        private Vector2 m_ScrollPosition = Vector2.zero;
        private float m_ScrollViewHeight = 0f;

        public Dialog_AddRecipe(in List<BulkRecipe> bulkRecipes, BulkProperties properties)
        {
            forcePause = true;
            doCloseButton = true;
            closeOnAccept = true;
            closeOnCancel = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            m_BulkRecipes = bulkRecipes;
            m_Properties = properties;
        }

        public override Vector2 InitialSize => new Vector2(600f, 800f);

        private List<RecipeDef> GetShowableRecipeList()
        {
            bool database = DefDatabase<RecipeDef>.AllDefsListForReading.Count != m_LastDefsCount;
            bool searchbox = m_SearchboxBuffer != m_LastSearchboxBuffer;

            if (m_CachedShowableRecipes == null || database || searchbox)
            {
                m_LastSearchboxBuffer = m_SearchboxBuffer;
                m_LastDefsCount = DefDatabase<RecipeDef>.AllDefsListForReading.Count;

                m_CachedShowableRecipes = DefDatabase<RecipeDef>.AllDefs
                .Where(recipe => BulkRecipe.CanBeBulk(recipe))
                .Where(recipe => !m_BulkRecipes.Select(bRecipe => bRecipe.DefName).Contains(recipe.defName))
                .Where(recipe => m_SearchboxBuffer.NullOrEmpty() || recipe.label.ToLower().Contains(m_SearchboxBuffer.ToLower()))
                .Where(recipe => m_SearchboxBuffer.NullOrEmpty() || recipe.defName.ToLower().Contains(m_SearchboxBuffer.ToLower()))
                .ToList();
            }

            return m_CachedShowableRecipes;
        }

        public override void DoWindowContents(Rect canva)
        {
            Rect topRect = canva.TopPart(0.125f).BottomPart(0.9f);
            Rect bottomRect = canva.BottomPart(0.85f).TopPart(0.9f);

            Rect titleRect = topRect.TopPart(0.4f);
            Rect searchRect = topRect.BottomPart(0.55f).TopPart(0.65f);
            Rect recipeRect = bottomRect;

            ShowTitleLabel(titleRect);
            ShowSearchBox(searchRect);

            Widgets.DrawLineHorizontal(topRect.x, topRect.yMax, topRect.width, Widgets.SeparatorLineColor);

            ShowRecipeList(recipeRect);
        }

        private void ShowTitleLabel(Rect titleRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, MYB_Data.AddRecipe_Button);
            Text.Font = GameFont.Small;
        }

        private void ShowSearchBox(Rect searchRect)
        {
            Rect searchboxRect = searchRect.LeftPart(0.65f);
            Rect searchLabelRect = searchRect.RightPart(0.325f).BottomPart(0.8f);

            m_SearchboxBuffer = Widgets.TextField(searchboxRect, m_SearchboxBuffer);

            int recipeCount = GetShowableRecipeList().Count;
            string recipeCountStr =
                recipeCount <= 10000 ? recipeCount.ToString() :
                $"{recipeCount / 1000}k";

            Widgets.Label(searchLabelRect, $"{MYB_Data.RecipesCount_Label}: {recipeCountStr}");
        }

        private void ShowRecipeList(Rect recipeRect)
        {
            List<RecipeDef> recipes = GetShowableRecipeList();

            Rect outRect = new Rect(Vector2.zero, recipeRect.size);
            Rect viewRect = new Rect(Vector2.zero, new Vector2(recipeRect.width - MYB_Data.GapX, m_ScrollViewHeight));

            GUI.BeginGroup(recipeRect);
            Widgets.BeginScrollView(outRect, ref m_ScrollPosition, viewRect, true);

            const float height = 64f;
            float currentHeight = 0f;
            foreach (RecipeDef recipe in recipes)
            {
                Rect entryRect = new Rect(0f, currentHeight, viewRect.width, height);
                Rect gapRect = entryRect.BottomPart(0.1f);

                ShowRecipeEntry(entryRect, recipe);

                Widgets.DrawLineHorizontal(gapRect.x, gapRect.y, gapRect.width, Widgets.SeparatorLineColor);

                currentHeight += height;
            }
            m_ScrollViewHeight = currentHeight;

            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private void ShowRecipeEntry(Rect entryRect, RecipeDef recipe)
        {
            Rect iconRect = new Rect(MYB_Data.GapX, entryRect.y, entryRect.height / 1.5f, entryRect.height / 1.5f);
            Rect labelRect = new Rect(new Vector2(iconRect.xMax + MYB_Data.GapX, entryRect.y + 5f), entryRect.LeftHalf().size);
            Rect clickableRect = entryRect.TopPart(0.75f);

            Widgets.ThingIcon(iconRect, recipe.ProducedThingDef);
            Widgets.Label(labelRect, recipe.LabelCap);

            if (Mouse.IsOver(clickableRect))
                Widgets.DrawHighlight(clickableRect);

            if (Widgets.ButtonInvisible(clickableRect))
            {
                m_BulkRecipes.Add(new BulkRecipe(recipe, m_Properties));
                base.Close();
            }
        }
    }
}