using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using Verse;

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
            ShowCheckboxes(canva);
            ShowButtons(canva);
            ShowTextFields(canva);

            ShowRecipesList(canva);

            ShowAttentionLabel(canva);
        }

        private void ShowCheckboxes(Rect canva)
        {
            Vector2 size = new Vector2(canva.center.x - 50f, 30f);

            Rect verboseLoggingRect = new Rect(MYB_Data.DefaultSpace, canva.y, size.x, size.y);
            Widgets.CheckboxLabeled(verboseLoggingRect, MYB_Data.VerboseLogging_Label, ref verboseLogging);
            if (Mouse.IsOver(verboseLoggingRect))
            {
                TooltipHandler.TipRegion(verboseLoggingRect, MYB_Data.VerboseLogging_Tooltip);
            }

            Rect addUnfinishedThingRect = new Rect(MYB_Data.DefaultSpace + verboseLoggingRect.xMax, canva.y, size.x, size.y);
            Widgets.CheckboxLabeled(addUnfinishedThingRect, MYB_Data.AddBulkUnfinishedThing_Label, ref addUnfinishedThing);
            if (Mouse.IsOver(addUnfinishedThingRect))
            {
                TooltipHandler.TipRegion(addUnfinishedThingRect, MYB_Data.AddBulkUnfinishedThing_Tooltip);
            }

            Rect sameQualityRect = new Rect(MYB_Data.DefaultSpace, canva.y + MYB_Data.DefaultSpace * 1.5f, size.x, size.y);
            Widgets.CheckboxLabeled(sameQualityRect, MYB_Data.SameQuality_Label, ref sameQuality);
            if (Mouse.IsOver(sameQualityRect))
            {
                TooltipHandler.TipRegion(sameQualityRect, MYB_Data.SameQuality_Tooltip);
            }
        }

        private void ShowButtons(Rect canva)
        {
            float start = 70f;
            Vector2 size = MYB_Data.ThirdSize(canva.width);

            Rect addButtonRect = new Rect(canva.x + MYB_Data.DefaultSpace, canva.y + start, size.x, size.y);
            if (Widgets.ButtonText(addButtonRect, MYB_Data.AddRecipe_Button))
            {
                AddButtonPressed();
            }
            Rect removeButtonRect = new Rect(canva.center.x - (size.x / 2f), canva.y + start, size.x, size.y);
            if (Widgets.ButtonText(removeButtonRect, MYB_Data.RemoveRecipe_Button))
            {
                RemoveButtonPressed();
            }
            Rect saveloadListRect = new Rect(canva.center.x - (size.x / 3.5f), canva.y + start + 40f, size.x / (3.5f / 2f), size.y);
            if (Widgets.ButtonText(saveloadListRect, MYB_Data.SaveLoad_Button))
            {
                SaveLoadButtonPressed();
            }
            Rect resetButtonRect = new Rect(canva.xMax - size.x - MYB_Data.DefaultSpace, canva.y + start, size.x, size.y);
            if (Widgets.ButtonText(resetButtonRect, MYB_Data.Reset_Button))
            {
                ResetButtonPressed();
            }
        }

        private void AddButtonPressed()
        {
            Find.WindowStack.Add(new Dialog_AddRecipe(recipe =>
            {
                bulkRecipes.Add(new BulkRecipe(recipe));
            }));
        }

        private void RemoveButtonPressed()
        {
            if (bulkRecipes.NullOrEmpty())
            {
                return;
            }

            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (BulkRecipe bulkRecipe in bulkRecipes)
            {
                list.Add(new FloatMenuOption(bulkRecipe.Label, delegate
                {
                    removedRecipes.Add(bulkRecipe.DefName);

                    bulkRecipes.Remove(bulkRecipe);
                }));
            }

            FloatMenu menu = new FloatMenu(list);
            Find.WindowStack.Add(menu);
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

                        if (replace)
                        {
                            Action replaceList = delegate { bulkRecipes = backupList.bulkRecipes.ToList(); };

                            if (!bulkRecipes.Empty())
                            {
                                bool isSaved = false;
                                foreach (ExposableBackupList bl in backupLists)
                                {
                                    if (BulkRecipe.CompareLists(bulkRecipes, bl.bulkRecipes))
                                    {
                                        isSaved = true;
                                        replaceList();
                                        break;
                                    }
                                }

                                if (!isSaved)
                                {
                                    Find.WindowStack.Add
                                    (
                                        new Dialog_MessageBox
                                        (
                                            MYB_Data.LoadListDialog_Message(backupList.listName), MYB_Data.Confirm_Button, replaceList,
                                            MYB_Data.Cancel_Button, null, MYB_Data.LoadListDialog_Title(backupList.listName), true
                                        )
                                    );
                                }

                            }
                            else
                            {
                                replaceList();
                            }
                        }
                        else
                        {
                            bulkRecipes.AddRange(backupList.bulkRecipes.ToList());
                        }
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

            Action onConfirm = delegate
            {
                foreach (BulkRecipe bulkRecipe in bulkRecipes)
                {
                    removedRecipes.Add(bulkRecipe.DefName);
                }
                bulkRecipes.Clear();
            };

            Find.WindowStack.Add(new Dialog_MessageBox(MYB_Data.ResetDialog_Message, MYB_Data.Confirm_Button, onConfirm, MYB_Data.Cancel_Button, null, MYB_Data.Reset_Button, true));
        }

        private void ShowTextFields(Rect canva)
        {
            float start = 40f + 70f;
            Vector2 size = MYB_Data.ThirdSize(canva.width);

            Rect searchBoxRect = new Rect(canva.x + MYB_Data.DefaultSpace, canva.y + start, size.x, size.y);
            searchBoxBuffer = Widgets.TextField(searchBoxRect, searchBoxBuffer);

            Rect searchTextRect = new Rect(searchBoxRect.xMax + 5f, searchBoxRect.center.y - 10f, 200f, size.y);
            Widgets.Label(searchTextRect, MYB_Data.SearchBox_Label);
        }

        private void ShowRecipesList(Rect canva)
        {
            float start = canva.y + 70f + 80f;
            float height = 100f + (MYB_Data.DefaultSpace * 2f);

            List<BulkRecipe> showableRecipes = new List<BulkRecipe>();
            foreach (BulkRecipe bulkRecipe in bulkRecipes)
            {
                if (bulkRecipe == null)
                {
                    bulkRecipes.Remove(bulkRecipe);
                    continue;
                }

                bulkRecipe.AdjustRecipeDef();
                if (bulkRecipe.recipeDef == null)
                {
                    bulkRecipes.Remove(bulkRecipe);
                    continue;
                }

                if (!bulkRecipe.Label.ToLower().Contains(searchBoxBuffer.ToLower()) && !searchBoxBuffer.NullOrEmpty())
                {
                    continue;
                }

                showableRecipes.Add(bulkRecipe);
            }

            Rect outRect = new Rect(canva.x, start, canva.width, canva.height - 175f);
            Rect scrollRect = new Rect(MYB_Data.DefaultSpace, canva.y + start, canva.width - MYB_Data.DefaultSpace, showableRecipes.Count * height);
            Widgets.BeginScrollView(outRect, ref scrollPosition, scrollRect);

            int index = 0;
            foreach (BulkRecipe bulkRecipe in showableRecipes)
            {
                float recipeStart = (MYB_Data.DefaultSpace * 2f) + start + (index * height);

                Listing_Standard listing = new Listing_Standard();
                listing.Begin(new Rect(MYB_Data.DefaultSpace, recipeStart, canva.width, canva.height));
                listing.GapLine();
                listing.End();

                Rect thingIconRect = new Rect(MYB_Data.DefaultSpace, MYB_Data.DefaultSpace + recipeStart, 64f, 64f);
                Widgets.ThingIcon(thingIconRect, bulkRecipe.recipeDef.ProducedThingDef);
                Rect labelRect = new Rect(MYB_Data.DefaultSpace + 70f, recipeStart + MYB_Data.DefaultSpace, 300f, 30f);
                Widgets.Label(labelRect, bulkRecipe.Label);

                float posY = recipeStart + 100f;
                Vector2 size = new Vector2((canva.width - (MYB_Data.DefaultSpace * 4f)) / 3f, 30f);

                FloatRange range = new FloatRange(0.1f, 2f);
                float roundTo = 0.05f;

                Rect productsRect = new Rect(0f, posY, size.x, size.y);
                Widgets.TextFieldNumericLabeled(productsRect, $"{MYB_Data.RecipeProducts_Label}   ", ref bulkRecipe.prop.products, ref bulkRecipe.prop.productsBuffer, 1, 1E+05f);

                Rect workAmountRect = new Rect(canva.center.x - (size.x / 2f) + MYB_Data.DefaultSpace, posY, size.x, size.y);
                string workAmountLabel = $"{MYB_Data.RecipeWorkAmount_Label} {bulkRecipe.prop.workAmount.ToStringPercent()}";
                Widgets.HorizontalSlider(workAmountRect, ref bulkRecipe.prop.workAmount, range, workAmountLabel, roundTo);

                Rect costRect = new Rect(canva.xMax - size.x, posY, size.x, size.y);
                string costLabel = $"{MYB_Data.RecipeCost_Label} {bulkRecipe.prop.cost.ToStringPercent()}";
                Widgets.HorizontalSlider(costRect, ref bulkRecipe.prop.cost, range, costLabel, roundTo);

                index++;
            }

            Widgets.EndScrollView();
        }

        private void ShowAttentionLabel(Rect canva)
        {
            Rect attentionRect = new Rect(canva.x, canva.yMax - 30f, canva.width, 30f);
            Widgets.Label(attentionRect, MYB_Data.Attention_Label);
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
