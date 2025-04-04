using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

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
            float space = 20f;
            Vector2 buttonSize = new Vector2(70f, 30f);

            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(canva.x, canva.y, canva.width, buttonSize.y);
            Widgets.Label(titleRect, MYB_Data.Load_Option);
            Text.Font = GameFont.Small;

            float start = canva.y + 20f;
            if (backupLists.NullOrEmpty())
            {
                Widgets.Label(new Rect(canva.x, start + space, canva.width, buttonSize.y), MYB_Data.LoadListEmpty_Label);
                return;
            }

            Rect scrollRect = new Rect(canva.x, titleRect.yMax + 10f, canva.width, canva.height - 100f);
            Rect viewRect = new Rect(canva.x, canva.y, scrollRect.width - space, (backupLists.Count * 40f) + space);
            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            foreach (ExposableBackupList backupList in backupLists)
            {
                GUI.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
                Widgets.DrawLineHorizontal(0f, start, canva.width);
                GUI.color = Color.white;

                Rect listNameRect = new Rect(canva.x + 12.5f, start + 5f, 300f, buttonSize.y);
                Widgets.Label(listNameRect, backupList.listName);

                Rect deleteButtonRect = new Rect(viewRect.width - buttonSize.y, start + 5f, buttonSize.y, buttonSize.y);
                if (Widgets.ButtonImage(deleteButtonRect, TexButton.Delete))
                {
                    Find.WindowStack.Add(new Dialog_MessageBox
                    (
                        MYB_Data.DeleteListDialog_Message(backupList.listName), MYB_Data.Confirm_Button, delegate { backupLists.Remove(backupList); },
                        MYB_Data.Cancel_Button, null, MYB_Data.DeleteListDialog_Title(backupList.listName), true
                    ));
                }
                Rect loadButtonRect = new Rect(deleteButtonRect.x - buttonSize.x - 5f, start + 5f, buttonSize.x, buttonSize.y);
                if (Widgets.ButtonText(loadButtonRect, MYB_Data.LoadList_Button))
                {
                    onLoad?.Invoke(backupList, true);
                    base.Close();
                }
                Rect addButtonRect = new Rect(loadButtonRect.x - buttonSize.x - 5f, start + 5f, buttonSize.x, buttonSize.y);
                if (Widgets.ButtonText(addButtonRect, MYB_Data.AddList_Button))
                {
                    onLoad?.Invoke(backupList, false);
                    base.Close();
                }

                start += 40f;
            }
            Widgets.EndScrollView();
        }
    }
}
