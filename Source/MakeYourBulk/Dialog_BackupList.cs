using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MakeYourBulk
{
    public class Dialog_SaveList : Window
    {
        private readonly Action<string> onSave;

        private string listName = MYB_Data.DefaultListName;

        public Dialog_SaveList(Action<string> onSave)
        {
            forcePause = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            this.onSave = onSave;
        }

        public override Vector2 InitialSize => new Vector2(400f, 125f);

        public override void DoWindowContents(Rect canva)
        {
            float space = 20f;
            Vector2 buttonSize = new Vector2(50f, 30f);

            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(canva.x, canva.y, canva.width, buttonSize.y);
            Widgets.Label(titleRect, MYB_Data.Save_Option);
            Text.Font = GameFont.Small;

            Rect nameFieldRect = new Rect(canva.x, canva.y + space * 2f, canva.width - buttonSize.x - space, buttonSize.y);
            listName = Widgets.TextField(nameFieldRect, listName, 24);

            Rect saveButtonRect = new Rect(nameFieldRect.xMax + space, nameFieldRect.y, buttonSize.x, buttonSize.y);
            if (Widgets.ButtonText(saveButtonRect, MYB_Data.SaveList_Button))
            {
                if (!listName.NullOrEmpty())
                {
                    OnAcceptKeyPressed();
                }
            }
        }

        public override void OnAcceptKeyPressed()
        {
            onSave?.Invoke(listName);
            base.OnAcceptKeyPressed();
        }
    }

    public class Dialog_LoadList : Window
    {
        private readonly List<ExposableBackupList> backupLists;
        private readonly Action<ExposableBackupList, bool> onLoad;

        private Vector2 scrollPosition = Vector2.zero;

        public Dialog_LoadList(List<ExposableBackupList> backupLists, Action<ExposableBackupList, bool> onLoad)
        {
            forcePause = true;
            doCloseButton = true;
            closeOnAccept = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            this.backupLists = backupLists;
            this.onLoad = onLoad;
        }

        public override Vector2 InitialSize => new Vector2(500f, 400f);

        public override void DoWindowContents(Rect canva)
        {
            Vector2 size = new Vector2(70f, 30f);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(canva);

            Text.Font = GameFont.Medium;
            Rect currentRow = listing.GetRect(Text.LineHeight + 4f);

            Rect titleRect = new Rect(currentRow.x, currentRow.y, currentRow.width, size.y);
            Widgets.Label(titleRect, MYB_Data.Load_Option);
            Text.Font = GameFont.Small;

            listing.GapLine();
            currentRow = listing.GetRect(250f);
            if (backupLists.NullOrEmpty())
            {
                Widgets.Label(new Rect(currentRow.x, currentRow.y, currentRow.width, size.y), MYB_Data.LoadListEmpty_Label);

                listing.End();
                return;
            }

            float height = 40f;
            Rect scrollRect = new Rect(currentRow.x, currentRow.y, currentRow.width - MYB_Data.DefaultSpace, backupLists.Count * (height + 12f));
            Widgets.BeginScrollView(currentRow, ref scrollPosition, scrollRect);

            Listing_Standard scrollListing = new Listing_Standard();
            scrollListing.Begin(scrollRect);

            foreach (ExposableBackupList backupList in backupLists)
            {
                currentRow = scrollListing.GetRect(height);

                Rect listNameRect = new Rect(currentRow.x, currentRow.y, currentRow.width, size.y);
                Widgets.Label(listNameRect, backupList.listName);

                float iconSize = 32f;
                Rect deleteButtonRect = new Rect(currentRow.xMax - iconSize, currentRow.y, iconSize, iconSize);
                if (Widgets.ButtonImage(deleteButtonRect, TexButton.Delete))
                {
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    Find.WindowStack.Add(new Dialog_MessageBox
                    (
                        MYB_Data.DeleteListDialog_Message(backupList.listName), MYB_Data.Confirm_Button, delegate { backupLists.Remove(backupList); },
                        MYB_Data.Cancel_Button, null, MYB_Data.DeleteListDialog_Title(backupList.listName), true
                    ));
                }
                Rect loadButtonRect = new Rect(deleteButtonRect.x - size.x, currentRow.y, size.x, size.y);
                if (Widgets.ButtonText(loadButtonRect, MYB_Data.LoadList_Button))
                {
                    onLoad?.Invoke(backupList, true);
                    base.Close();
                }
                Rect addButtonRect = new Rect(loadButtonRect.x - size.x, currentRow.y, size.x, size.y);
                if (Widgets.ButtonText(addButtonRect, MYB_Data.AddList_Button))
                {
                    onLoad?.Invoke(backupList, false);
                    base.Close();
                }

                scrollListing.GapLine();
            }
            Widgets.EndScrollView();

            scrollListing.End();

            listing.GapLine();
            listing.End();
        }
    }
}
