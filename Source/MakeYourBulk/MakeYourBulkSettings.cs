using Force.DeepCloner;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MakeYourBulk
{
    [HotSwappable]
    public class MakeYourBulkSettings : ModSettings
    {
        private List<BulkRecipe> m_BulkRecipes = new List<BulkRecipe>();
        private List<ExposableBackupList> m_BackupLists = new List<ExposableBackupList>();

        private List<BulkRecipe> m_CachedShowableRecipes = null;
        private int m_LastDefsCount = 0;

        private string m_SearchboxBuffer = "";

        private Vector2 m_ScrollPosition = Vector2.zero;
        private float m_ScrollViewHeight = 0f;
        private float m_HeightLevel;

        private bool m_VerboseLogging;
        public bool VerboseLogging => m_VerboseLogging;
        private bool m_AddUnfinishedThing;
        private bool m_SameQuality;

        private List<BulkRecipe> GetShowableBulkRecipes()
        {
            if (m_CachedShowableRecipes.NullOrEmpty() || m_BulkRecipes.Count != m_LastDefsCount)
            {
                m_LastDefsCount = m_BulkRecipes.Count;

                m_CachedShowableRecipes = m_BulkRecipes
                .Where(recipe => recipe.GetBaseRecipe() != null)
                .Where(recipe => m_SearchboxBuffer.NullOrEmpty() || recipe.RealLabel.ToLower().Contains(m_SearchboxBuffer.ToLower()))
                .ToList();
            }

            return m_CachedShowableRecipes;
        }

        public void DoWindowContents(Rect canva)
        {
            Rect topPart = canva.TopPart(0.2f).BottomPart(0.9f);
            Rect bottomPart = canva.BottomPart(0.79f);

            Rect checkboxRect = topPart.TopPart(0.3f).CenteredOnXIn(topPart.ContractedBy(MYB_Data.GapX));
            Rect buttonRect = topPart.BottomPart(0.7f).TopPart(0.4f);
            Rect searchsizeRect = topPart.BottomPart(0.35f).TopPart(0.8f);

            Rect recipeRect = bottomPart.TopPart(0.85f);
            Rect attentionRect = bottomPart.BottomPart(0.1f);

            // TODO: Default Products, Work Amount, Cost

            ShowCheckboxes(checkboxRect);
            ShowButtons(buttonRect);
            ShowSearchboxAndSize(searchsizeRect);

            Widgets.DrawLineHorizontal(0f, topPart.yMax, canva.width, Widgets.SeparatorLabelColor);

            ShowRecipeList(recipeRect);
            ShowAttentionLabel(attentionRect);
        }

        private void ShowCheckboxes(Rect checkboxRect)
        {
            Listing_Standard listing = new Listing_Standard
            {
                ColumnWidth = checkboxRect.width / 3.1215f,
            };

            listing.Begin(checkboxRect);

            listing.CheckboxLabeled(MYB_Data.VerboseLogging_Label, ref m_VerboseLogging, MYB_Data.VerboseLogging_Tooltip);
            listing.NewColumn();
            listing.CheckboxLabeled(MYB_Data.AddBulkUnfinishedThing_Label, ref m_AddUnfinishedThing, MYB_Data.AddBulkUnfinishedThing_Tooltip);
            listing.NewColumn();
            listing.CheckboxLabeled(MYB_Data.SameQuality_Label, ref m_SameQuality, MYB_Data.SameQuality_Tooltip);

            listing.End();
        }

        private void ShowButtons(Rect buttonRect)
        {
            Rect addRect = MYB_Data.LeftThird(buttonRect);
            Rect saveloadRect = MYB_Data.MiddleThird(buttonRect);
            Rect resetRect = MYB_Data.RightThird(buttonRect);

            if (Widgets.ButtonText(addRect, MYB_Data.AddRecipe_Button))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                Find.WindowStack.Add(new Dialog_AddRecipe(m_BulkRecipes));
            }

            if (Widgets.ButtonText(saveloadRect, MYB_Data.SaveLoad_Button))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                SaveLoadButtonPressed();
            }

            if (Widgets.ButtonText(resetRect, MYB_Data.Reset_Button))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                ResetButtonPressed();
            }
        }

        private void SaveLoadButtonPressed()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>
            {
                new FloatMenuOption(MYB_Data.Save_Option, delegate
                {
                    if (m_BulkRecipes.NullOrEmpty())
                    {
                        Messages.Message(MYB_Data.EmptyList_Message, MessageTypeDefOf.RejectInput, false);
                        return;
                    }

                    Find.WindowStack.Add(new Dialog_SaveList(m_BackupLists, m_BulkRecipes));
                }),
                new FloatMenuOption(MYB_Data.Load_Option, delegate
                {
                    if (m_BackupLists.NullOrEmpty())
                    {
                        Messages.Message(MYB_Data.LoadListEmpty_Label, MessageTypeDefOf.RejectInput, false);
                        return;
                    }

                    Find.WindowStack.Add(new Dialog_LoadList(m_BackupLists, m_BulkRecipes));
                })
            };

            FloatMenu menu = new FloatMenu(options);
            Find.WindowStack.Add(menu);
        }

        private void ResetButtonPressed()
        {
            if (m_BulkRecipes.NullOrEmpty())
                return;

            void cbResetList() => m_BulkRecipes.Clear();

            if (HasBackupList())
            {
                cbResetList();
                return;
            }

            Find.WindowStack.Add(new Dialog_MessageBox(MYB_Data.ResetDialog_Message,
                    MYB_Data.Confirm_Button, cbResetList,
                    MYB_Data.Cancel_Button, null, MYB_Data.Reset_Button,
                    true));
        }

        private void ShowSearchboxAndSize(Rect rect)
        {
            Rect searchBoxRect = MYB_Data.LeftThird(rect);
            Rect searchLabelRect = MYB_Data.MiddleThird(rect).BottomPart(0.7f).CenteredOnYIn(rect);

            Rect sizeRect = MYB_Data.RightThird(rect, 0.9f);

            m_SearchboxBuffer = Widgets.TextField(searchBoxRect, m_SearchboxBuffer);
            Widgets.Label(searchLabelRect, $"{MYB_Data.SearchBox_Label} ({GetShowableBulkRecipes().Count}/{m_BulkRecipes.Count})");

            Widgets.HorizontalSlider(sizeRect, ref m_HeightLevel, new FloatRange(1, 10), $"Height: {m_HeightLevel}", 1);
        }

        private void ShowRecipeList(Rect recipeRect)
        {
            List<BulkRecipe> showableRecipes = GetShowableBulkRecipes();
            if (showableRecipes.NullOrEmpty())
                m_ScrollViewHeight = 0f;

            Rect outRect = new Rect(Vector2.zero, recipeRect.size);
            Rect viewRect = new Rect(Vector2.zero, new Vector2(recipeRect.width - MYB_Data.GapX, m_ScrollViewHeight));

            GUI.BeginGroup(recipeRect);
            Widgets.BeginScrollView(outRect, ref m_ScrollPosition, viewRect, true);

            float entryHeight = 80f + (m_HeightLevel - 1) * 5f;
            int startRecipe = (int)Math.Max(Math.Floor(m_ScrollPosition.y / entryHeight) - 1, 0);
            int endRecipes = startRecipe + (int)Math.Ceiling(outRect.height / entryHeight) + 1;

            float currentHeight = 0f;
            for (int i = 0; i < m_BulkRecipes.Count; i++)
            {
                Rect entryRect = new Rect(MYB_Data.GapX, currentHeight + MYB_Data.GapY, viewRect.width - MYB_Data.GapX, entryHeight);

                Widgets.DrawLineHorizontal(0f, currentHeight, viewRect.width, Widgets.SeparatorLineColor);

                if (i >= startRecipe && i <= endRecipes)
                    ShowRecipeEntry(entryRect, i);

                currentHeight += entryHeight;
            }
            m_ScrollViewHeight = currentHeight;

            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private void ShowRecipeEntry(Rect entryRect, int index)
        {
            BulkRecipe recipe = GetShowableBulkRecipes().ElementAt(index);
            RecipeDef recipeDef = recipe.GetBulkRecipeDef(m_AddUnfinishedThing, m_SameQuality);

            float iconSize = entryRect.height / 2f;
            Rect indexRect = new Rect(0f, entryRect.y, entryRect.width, entryRect.height);
            Rect iconRect = new Rect(entryRect.x, entryRect.y, iconSize, iconSize);
            Rect labelRect = new Rect(new Vector2(iconRect.xMax + MYB_Data.GapX, entryRect.y + MYB_Data.GapY), entryRect.LeftHalf().size);

            float toolSize = entryRect.height / 3f;
            Rect removeRect = new Rect(entryRect.xMax - toolSize, entryRect.y, toolSize, toolSize);
            Rect copyRect = new Rect(removeRect.x - toolSize - MYB_Data.GapY, entryRect.y, toolSize, toolSize);
            Rect renameRect = new Rect(copyRect.x - toolSize - MYB_Data.GapY, entryRect.y, toolSize, toolSize);

            Rect bottomPart = entryRect.BottomPart(0.4f).TopPart(0.6f);
            Rect productRect = MYB_Data.LeftThird(bottomPart).LeftPart(0.8f);
            Rect workRect = MYB_Data.MiddleThird(bottomPart, 0.9f);
            Rect costRect = MYB_Data.RightThird(bottomPart, 0.9f);

            Text.Font = GameFont.Tiny;
            Widgets.Label(indexRect, index.ToString());
            Text.Font = GameFont.Small;

            Widgets.ThingIcon(iconRect, recipe.ProductThingDef);
            if (Mouse.IsOver(iconRect))
                TooltipHandler.TipRegion(iconRect, recipe.BaseLabel);

            Widgets.Label(labelRect, recipe.RealLabel);

            if (Widgets.ButtonImage(renameRect, TexButton.Rename))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                Find.WindowStack.Add(new Dialog_RenameBulkRecipe(recipe));
            }
            if (Widgets.ButtonImage(removeRect, TexButton.Delete))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                m_BulkRecipes.Remove(recipe);
            }
            if (Widgets.ButtonImage(copyRect, TexButton.Copy))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                m_BulkRecipes.Add(recipe.ShallowClone());
            }

            Widgets.TextFieldNumericLabeled(productRect, $"{MYB_Data.RecipeProducts_Label}   ",
                ref recipe._Properties._Product, ref recipe._Properties._ProductBuffer, 1, 1E+05f);

            FloatRange range = new FloatRange(0.1f, 2f);
            float roundTo = 0.05f;

            string workAmountLabel = $"{MYB_Data.RecipeWorkAmount_Label} {recipe.WorkAmountPercent}";
            Widgets.HorizontalSlider(workRect, ref recipe._Properties._WorkAmount, range, workAmountLabel, roundTo);
            if (Mouse.IsOver(workRect))
                TooltipHandler.TipRegion(workRect, $"{MYB_Data.RecipeWorkAmount_Label}: {recipeDef.workAmount}");

            string costLabel = $"{MYB_Data.RecipeCost_Label} {recipe.CostPercent}";
            Widgets.HorizontalSlider(costRect, ref recipe._Properties._Cost, range, costLabel, roundTo);
            if (Mouse.IsOver(costRect))
            {
                string costTooltip = $"{MYB_Data.RecipeCost_Label}: ";
                recipeDef.ingredients.Select(ingredient => ingredient.Summary)
                    .ToList().ForEach(ingredient => costTooltip += $"{ingredient}, ");
                costTooltip = costTooltip.Substring(0, costTooltip.Length - 2);
                TooltipHandler.TipRegion(costRect, costTooltip);
            }
        }

        private void ShowAttentionLabel(Rect attentionRect)
        {
            Rect labelRect = attentionRect.BottomPart(0.9f);

            Widgets.DrawLineHorizontal(0f, attentionRect.y, attentionRect.width, Widgets.SeparatorLabelColor);
            Widgets.Label(labelRect, MYB_Data.Attention_Label);
        }

        private bool HasBackupList()
        {
            bool isSaved = false;
            m_BackupLists.Select(list => list._BulkRecipes)
                .ToList().ForEach(list =>
                {
                    if (isSaved)
                        return;

                    isSaved = m_BulkRecipes.SequenceEqual(list);
                });

            return isSaved;
        }

        public void AddToDatabase()
        {
            foreach (BulkRecipe bulkRecipe in m_BulkRecipes)
            {
                if (bulkRecipe == null || bulkRecipe.GetBaseRecipe() == null)
                {
                    m_BulkRecipes.Remove(bulkRecipe);
                    continue;
                }

                if (DefDatabase<RecipeDef>.GetNamedSilentFail(bulkRecipe.DefName) == null)
                {
                    RecipeDef bulkRecipeDef = bulkRecipe.GetBulkRecipeDef(m_AddUnfinishedThing, m_SameQuality);
                    if (bulkRecipeDef == null)
                    {
                        MYB_Log.Error("Unexpected BulkRecipe is Null");
                        continue;
                    }

                    MYB_Log.Trace($"Adding '{bulkRecipe.DefName}' into DefDatabase<RecipeDef>");
                    DefDatabase<RecipeDef>.Add(bulkRecipeDef);

                    List<ThingDef> recipeUsers = DefDatabase<ThingDef>.AllDefs
                        .Where(recipeUser => recipeUser.recipes != null && recipeUser.recipes.Contains(bulkRecipe.GetBaseRecipe()))
                        .ToList();
                    if (!recipeUsers.NullOrEmpty())
                    {
                        string users = "";
                        recipeUsers.Select(recipeUser => recipeUser.LabelCap).ToList().ForEach(recipeUser => users += $"{recipeUser}, ");
                        users = users.Substring(0, users.Length - 2);
                        MYB_Log.Trace($"Adding '{bulkRecipe.DefName}' into {users}");
                        foreach (ThingDef recipeUser in recipeUsers)
                        {
                            recipeUser.recipes.Add(bulkRecipeDef);
                        }
                    }
                }
            }
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref m_BackupLists, MYB_Data.Settings_BackupList, LookMode.Deep);
            Scribe_Collections.Look(ref m_BulkRecipes, MYB_Data.Settings_Recipes, LookMode.Deep);

            Scribe_Values.Look(ref m_VerboseLogging, MYB_Data.Settings_VerboseLogging, MYB_Data.Settings_DefaultVerboseLogging);
            Scribe_Values.Look(ref m_AddUnfinishedThing, MYB_Data.Settings_AddUnfinishedThing, MYB_Data.Settings_DefaultAddUnfishedThing);
            Scribe_Values.Look(ref m_SameQuality, MYB_Data.Settings_SameQuality, MYB_Data.Settings_DefaultSameQuality);

            Scribe_Values.Look(ref m_HeightLevel, MYB_Data.Settings_HeightLevel, MYB_Data.Settings_DefaultHeightLevel);

            base.ExposeData();
        }
    }
}
