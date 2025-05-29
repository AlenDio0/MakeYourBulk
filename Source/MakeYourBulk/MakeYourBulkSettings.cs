using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MakeYourBulk
{
    public class MakeYourBulkSettings : ModSettings
    {
        private List<ExposableBackupList> backupLists = new List<ExposableBackupList>();

        public List<BulkRecipe> bulkRecipes = new List<BulkRecipe>();
        private readonly List<string> removedRecipes = new List<string>();

        private string searchBoxBuffer = "";

        private Vector2 scrollPosition = Vector2.zero;

        // Settings
        private bool verboseLogging;
        public bool VerboseLogging => verboseLogging;
        private bool addUnfinishedThing;
        private bool sameQuality;

        public void DoWindowContents(Rect canva)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(canva);

            ShowCheckboxes(listing);
            listing.GapLine();

            ShowButtons(listing);
            listing.Gap();

            ShowTextFields(listing);
            listing.Gap(24f);

            ShowRecipesList(listing);
            listing.Gap();

            ShowAttentionLabel(listing);

            listing.End();
        }

        private void ShowCheckboxes(Listing_Standard listing)
        {
            Rect currentRow = listing.GetRect(Text.LineHeight + 4f);
            Vector2 size = new Vector2(currentRow.center.x, 30f);

            Rect verboseLoggingRect = new Rect(currentRow.x, currentRow.y, size.x, size.y);
            Widgets.CheckboxLabeled(verboseLoggingRect, MYB_Data.VerboseLogging_Label, ref verboseLogging);
            if (Mouse.IsOver(verboseLoggingRect))
            {
                TooltipHandler.TipRegion(verboseLoggingRect, MYB_Data.VerboseLogging_Tooltip);
            }

            currentRow = listing.GetRect(Text.LineHeight + 4f);
            Rect addUnfinishedThingRect = new Rect(currentRow.x, currentRow.y, size.x, size.y);
            Widgets.CheckboxLabeled(addUnfinishedThingRect, MYB_Data.AddBulkUnfinishedThing_Label, ref addUnfinishedThing);
            if (Mouse.IsOver(addUnfinishedThingRect))
            {
                TooltipHandler.TipRegion(addUnfinishedThingRect, MYB_Data.AddBulkUnfinishedThing_Tooltip);
            }

            currentRow = listing.GetRect(Text.LineHeight + 4f);
            Rect sameQualityRect = new Rect(currentRow.x, currentRow.y, size.x, size.y);
            Widgets.CheckboxLabeled(sameQualityRect, MYB_Data.SameQuality_Label, ref sameQuality);
            if (Mouse.IsOver(sameQualityRect))
            {
                TooltipHandler.TipRegion(sameQualityRect, MYB_Data.SameQuality_Tooltip);
            }
        }

        private void ShowButtons(Listing_Standard listing)
        {
            Rect currentRow = listing.GetRect(Text.LineHeight + 4f);
            Vector2 size = MYB_Data.ThirdSize(currentRow.width);

            Rect addButtonRect = new Rect(currentRow.x, currentRow.y, size.x, size.y);
            if (Widgets.ButtonText(addButtonRect, MYB_Data.AddRecipe_Button))
            {
                AddButtonPressed();
            }
            Rect saveloadListRect = new Rect(currentRow.center.x - size.x / 2f, currentRow.y, size.x, size.y);
            if (Widgets.ButtonText(saveloadListRect, MYB_Data.SaveLoad_Button))
            {
                SaveLoadButtonPressed();
            }
            Rect resetButtonRect = new Rect(currentRow.xMax - size.x, currentRow.y, size.x, size.y);
            if (Widgets.ButtonText(resetButtonRect, MYB_Data.Reset_Button))
            {
                ResetButtonPressed();
            }
        }

        private void ShowTextFields(Listing_Standard listing)
        {
            TextAnchor defaultAnchor = Text.Anchor;

            Rect currentRow = listing.GetRect(Text.LineHeight + 4f);
            Vector2 size = MYB_Data.ThirdSize(currentRow.width);

            Rect searchBoxRect = new Rect(currentRow.x, currentRow.y, size.x, size.y);
            searchBoxBuffer = Widgets.TextField(searchBoxRect, searchBoxBuffer);

            Rect searchTextRect = new Rect(searchBoxRect.xMax + 5f, currentRow.y, 200f, size.y);
            Widgets.Label(searchTextRect, MYB_Data.SearchBox_Label);

            Text.Anchor = TextAnchor.MiddleRight;
            Rect countTextRect = new Rect(currentRow.xMax - size.x, currentRow.y, size.x, size.y);
            Widgets.Label(countTextRect, $"({GetShowableBulkRecipes().Count()}/{bulkRecipes.Count})");

            Text.Anchor = defaultAnchor;
        }

        private void ShowRecipesList(Listing_Standard listing)
        {
            List<BulkRecipe> showableRecipes = GetShowableBulkRecipes().ToList();

            Rect currentRow = listing.GetRect(350f);

            float height = 140f;
            Rect scrollRect = new Rect(currentRow.x, currentRow.y, currentRow.width - MYB_Data.DefaultSpace, showableRecipes.Count * height);
            Widgets.BeginScrollView(currentRow, ref scrollPosition, scrollRect);

            Listing_Standard scrollListing = new Listing_Standard();
            scrollListing.Begin(scrollRect);

            foreach (BulkRecipe bulkRecipe in showableRecipes)
            {
                float iconSize = 64f;
                currentRow = scrollListing.GetRect(iconSize + MYB_Data.DefaultSpace);

                Rect thingIconRect = new Rect(currentRow.x, currentRow.y, iconSize, iconSize);
                Widgets.ThingIcon(thingIconRect, bulkRecipe.recipeDef.ProducedThingDef);

                Rect labelRect = new Rect(thingIconRect.xMax + MYB_Data.DefaultSpace * 2f, currentRow.y + MYB_Data.DefaultSpace, 300f, 30f);
                Widgets.Label(labelRect, bulkRecipe.Label);

                Rect removeRect = new Rect(currentRow.xMax - 48f - MYB_Data.DefaultSpace, currentRow.y, 48f, 48f);
                if (Widgets.ButtonImage(removeRect, TexButton.Delete))
                {
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    RemoveBulkRecipe(bulkRecipe);
                }


                Vector2 size = new Vector2((currentRow.width - (MYB_Data.DefaultSpace * 4f)) / 3f, 30f);
                currentRow = scrollListing.GetRect(size.y + MYB_Data.DefaultSpace);

                FloatRange range = new FloatRange(0.1f, 2f);
                float roundTo = 0.01f;

                Rect productsRect = new Rect(currentRow.x, thingIconRect.yMax + MYB_Data.DefaultSpace, size.x, size.y);
                Widgets.TextFieldNumericLabeled(productsRect, $"{MYB_Data.RecipeProducts_Label}   ", ref bulkRecipe.prop.products, ref bulkRecipe.prop.productsBuffer, 1, 1E+05f);

                Rect workAmountRect = new Rect(productsRect.xMax + MYB_Data.DefaultSpace, thingIconRect.yMax + MYB_Data.DefaultSpace, size.x, size.y);
                string workAmountLabel = $"{MYB_Data.RecipeWorkAmount_Label} {bulkRecipe.prop.workAmount.ToStringPercent()}";
                Widgets.HorizontalSlider(workAmountRect, ref bulkRecipe.prop.workAmount, range, workAmountLabel, roundTo);

                Rect costRect = new Rect(workAmountRect.xMax + MYB_Data.DefaultSpace, thingIconRect.yMax + MYB_Data.DefaultSpace, size.x, size.y);
                string costLabel = $"{MYB_Data.RecipeCost_Label} {bulkRecipe.prop.cost.ToStringPercent()}";
                Widgets.HorizontalSlider(costRect, ref bulkRecipe.prop.cost, range, costLabel, roundTo);

                scrollListing.GapLine(6f);
            }
            scrollListing.End();

            Widgets.EndScrollView();
        }

        private void ShowAttentionLabel(Listing_Standard listing)
        {
            Rect currentRow = listing.GetRect(Text.LineHeight + 4f);

            Rect attentionRect = new Rect(currentRow.x, currentRow.yMax - 30f, currentRow.width, 30f);
            Widgets.Label(attentionRect, MYB_Data.Attention_Label);
        }

        private void AddButtonPressed()
        {
            Find.WindowStack.Add(new Dialog_AddRecipe(recipe =>
            {
                bulkRecipes.Add(new BulkRecipe(recipe));
            }));
        }

        private void SaveLoadButtonPressed()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>
            {
                new FloatMenuOption(MYB_Data.Save_Option, delegate
                {
                    if (bulkRecipes.NullOrEmpty())
                    {
                        Messages.Message(MYB_Data.EmptyList_Message, MessageTypeDefOf.RejectInput, false);
                        return;
                    }

                    Find.WindowStack.Add(new Dialog_SaveList(name =>
                    {
                        if (backupLists == null)
                        {
                            backupLists = new List<ExposableBackupList>();
                        }

                        string newName = name;
                        if (!backupLists.NullOrEmpty())
                        {
                            int index = 1;
                            bool isUnique = true;
                            do
                            {
                                isUnique = true;
                                foreach (ExposableBackupList backupList in backupLists)
                                {
                                    if (backupList == null)
                                    {
                                        continue;
                                    }

                                    if (newName == backupList.listName)
                                    {
                                        isUnique = false;
                                        newName = $"{name} ({index})";
                                        index++;
                                        break;
                                    }
                                }
                            } while (!isUnique);
                        }

                        backupLists.Add(new ExposableBackupList(newName, bulkRecipes.ToList()));
                    }));
                }),
                new FloatMenuOption(MYB_Data.Load_Option, delegate
                {
                    Find.WindowStack.Add(new Dialog_LoadList(backupLists, (backupList, replace) =>
                    {
                        if (backupList == null)
                        {
                            return;
                        }

                        if (!replace)
                        {
                            bulkRecipes.AddRange(backupList.bulkRecipes.ToList());
                            return;
                        }

                        void replaceList()
                        {
                            foreach (BulkRecipe bulkRecipe in bulkRecipes.ToList())
                            {
                                RemoveBulkRecipe(bulkRecipe);
                            }
                            bulkRecipes = backupList.bulkRecipes.ToList();
                        }

                        if (IsListSaved() || bulkRecipes.Empty())
                        {
                            replaceList();
                            return;
                        }

                        Find.WindowStack.Add
                        (
                            new Dialog_MessageBox
                            (
                                MYB_Data.LoadListDialog_Message(backupList.listName), MYB_Data.Confirm_Button, replaceList,
                                MYB_Data.Cancel_Button, null, MYB_Data.LoadListDialog_Title(backupList.listName), true
                            )
                        );
                    }));
                    })
            };

            FloatMenu menu = new FloatMenu(list);
            Find.WindowStack.Add(menu);
        }

        private void ResetButtonPressed()
        {
            if (bulkRecipes.Count == 0)
            {
                return;
            }

            void resetList()
            {
                foreach (BulkRecipe bulkRecipe in bulkRecipes.ToList())
                {
                    RemoveBulkRecipe(bulkRecipe);
                }
            }

            if (IsListSaved())
            {
                resetList();
            }
            else
            {
                Find.WindowStack.Add(new Dialog_MessageBox(MYB_Data.ResetDialog_Message, MYB_Data.Confirm_Button, resetList, MYB_Data.Cancel_Button, null, MYB_Data.Reset_Button, true));
            }
        }

        private bool IsListSaved()
        {
            foreach (ExposableBackupList backupList in backupLists)
            {
                if (BulkRecipe.CompareLists(bulkRecipes, backupList.bulkRecipes))
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<BulkRecipe> GetShowableBulkRecipes()
        {
            foreach (BulkRecipe bulkRecipe in bulkRecipes)
            {
                if (bulkRecipe == null)
                {
                    continue;
                }

                bulkRecipe.AdjustRecipeDef();
                if (bulkRecipe.recipeDef == null)
                {
                    continue;
                }

                if (!bulkRecipe.Label.ToLower().Contains(searchBoxBuffer.ToLower()) && !searchBoxBuffer.NullOrEmpty())
                {
                    continue;
                }

                yield return bulkRecipe;
            }
        }

        private void RemoveBulkRecipe(BulkRecipe bulkRecipe)
        {
            removedRecipes.Add(bulkRecipe.DefName);
            bulkRecipes.Remove(bulkRecipe);
        }

        public void AddToDatabase()
        {
            if (bulkRecipes == null)
            {
                MYB_Log.Warn("BulkRecipesDatabase not initialized");
                return;
            }

            foreach (BulkRecipe bulkRecipe in bulkRecipes)
            {
                if (bulkRecipe == null)
                {
                    MYB_Log.Error("Removed a null bulk recipe");
                    bulkRecipes.Remove(bulkRecipe);
                    continue;
                }

                bulkRecipe.AdjustRecipeDef();
                if (bulkRecipe.recipeDef == null)
                {
                    MYB_Log.Warn($"The recipe '{bulkRecipe.RecipeDefName}' doesn't exists");
                    continue;
                }

                if (DefDatabase<RecipeDef>.GetNamedSilentFail(bulkRecipe.DefName) == null)
                {
                    RecipeDef bulkRecipeDef = bulkRecipe.CreateBulkRecipeDef(addUnfinishedThing, sameQuality);
                    if (bulkRecipeDef != null)
                    {
                        MYB_Log.Trace($"Adding '{bulkRecipe.DefName}' into DefDatabase<RecipeDef>");

                        DefDatabase<RecipeDef>.Add(bulkRecipeDef);
                    }
                }
            }
        }

        public void RemoveFromDatabase()
        {
            if (removedRecipes == null)
            {
                return;
            }

            foreach (string recipe in removedRecipes)
            {
                RecipeDef recipeDef = DefDatabase<RecipeDef>.GetNamedSilentFail(recipe);
                if (recipeDef != null)
                {
                    MYB_Log.Trace($"Removing '{recipe}' from DefDatabase<RecipeDef>, restart the game if it doesn't apply");
                    DefDatabase<RecipeDef>.AllDefsListForReading.Remove(recipeDef);
                }
            }
            removedRecipes.Clear();
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref backupLists, MYB_Data.Settings_BackupList, LookMode.Deep);
            Scribe_Collections.Look(ref bulkRecipes, MYB_Data.Settings_Recipes, LookMode.Deep);

            Scribe_Values.Look(ref verboseLogging, MYB_Data.Settings_VerboseLogging, MYB_Data.Settings_DefaultVerboseLogging);
            Scribe_Values.Look(ref addUnfinishedThing, MYB_Data.Settings_AddUnfinishedThing, MYB_Data.Settings_DefaultAddUnfishedThing);
            Scribe_Values.Look(ref sameQuality, MYB_Data.Settings_SameQuality, MYB_Data.Settings_DefaultSameQuality);

            base.ExposeData();
        }
    }
}
