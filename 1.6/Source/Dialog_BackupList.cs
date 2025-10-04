using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MakeYourBulk
{
    public class Dialog_SaveList : Window
    {
        private readonly List<ExposableBackupList> m_BackupLists;
        private readonly ExposableBackupList m_Backup = null;

        private string m_SearchboxBuffer = "";

        private Vector2 m_ScrollPosition = Vector2.zero;
        private float m_ScrollViewHeight = 0f;

        public Dialog_SaveList(List<ExposableBackupList> backupLists, List<BulkRecipe> bulkRecipes)
        {
            forcePause = true;
            doCloseButton = true;
            closeOnAccept = true;
            closeOnCancel = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            m_BackupLists = backupLists;
            m_Backup = new ExposableBackupList("", bulkRecipes);
        }

        public override Vector2 InitialSize => new Vector2(550f, 625f);

        private List<ExposableBackupList> GetShowableBackupLists()
        {
            return m_BackupLists
                .Where(backup => m_SearchboxBuffer.NullOrEmpty() || backup._ListName.ToLower().Contains(m_SearchboxBuffer.ToLower()))
                .ToList();
        }

        public override void DoWindowContents(Rect canva)
        {
            Rect topRect = canva.TopPart(0.8f);
            Rect bottomRect = canva.BottomPart(0.175f).TopPart(0.3f);

            Rect searchRect = topRect.TopPart(0.07f);
            Rect backupRect = topRect.BottomPart(0.875f);

            ShowSearchbox(searchRect);
            ShowBackupLists(backupRect);

            ShowSaveFieldAndButton(bottomRect);
        }

        private void ShowSearchbox(Rect searchRect)
        {
            Rect searchboxRect = searchRect.RightPart(0.9f).LeftPart(0.95f);
            Rect iconRect = searchRect.LeftPartPixels(32f);

            m_SearchboxBuffer = Widgets.TextField(searchboxRect, m_SearchboxBuffer);
            Widgets.ButtonImage(iconRect, TexButton.Search, Color.white, Color.white, false);
        }

        private void ShowBackupLists(Rect backupRect)
        {
            Rect outRect = new Rect(Vector2.zero, backupRect.size);
            Rect viewRect = new Rect(Vector2.zero, new Vector2(backupRect.width - MYB_Data.GapX, m_ScrollViewHeight));

            GUI.BeginGroup(backupRect);
            Widgets.BeginScrollView(outRect, ref m_ScrollPosition, viewRect, true);

            const float height = 35f;
            float currentHeight = 0f;
            int i = 0;
            foreach (ExposableBackupList backup in GetShowableBackupLists())
            {
                Rect entryRect = new Rect(0f, currentHeight, viewRect.width, height);

                if (i % 2 != 0)
                    Widgets.DrawBoxSolid(entryRect, new Color(1f, 1f, 1f, 0.1f));

                ShowBackupEntry(entryRect, backup);

                currentHeight += height + MYB_Data.GapY * 2f;
                i++;
            }
            m_ScrollViewHeight = currentHeight;

            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private void ShowBackupEntry(Rect entryRect, ExposableBackupList backup)
        {
            Rect rightRect = entryRect.RightPart(0.3f);

            Rect labelRect = entryRect.LeftHalf().RightPart(0.95f).BottomPart(0.85f);
            Rect overwriteRect = rightRect.LeftPart(0.75f);
            Rect deleteRect = rightRect.RightPartPixels(30f);

            Widgets.Label(labelRect, backup._ListName);

            if (Widgets.ButtonText(overwriteRect, MYB_Data.OverwriteList_Button))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                m_Backup._ListName = backup._ListName;
                m_BackupLists.Replace(backup, m_Backup);
                base.Close();
            }

            if (Widgets.ButtonImage(deleteRect, TexButton.Delete))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                Find.WindowStack.Add(new Dialog_MessageBox
                (
                    MYB_Data.DeleteListDialog_Message(backup._ListName), MYB_Data.Confirm_Button, delegate { m_BackupLists.Remove(backup); },
                    MYB_Data.Cancel_Button, null, MYB_Data.DeleteListDialog_Title(backup._ListName), true
                ));
            }
        }

        private void ShowSaveFieldAndButton(Rect saveRect)
        {
            Rect nameRect = saveRect.LeftPart(0.7f);
            Rect buttonRect = saveRect.RightPart(0.275f);

            m_Backup._ListName = Widgets.TextField(nameRect, m_Backup._ListName, 24);
            if (Widgets.ButtonText(buttonRect, MYB_Data.SaveList_Button))
                OnAcceptKeyPressed();
        }

        public override void OnAcceptKeyPressed()
        {
            SoundDefOf.Click.PlayOneShotOnCamera();
            if (m_Backup._ListName.NullOrEmpty())
                return;

            string newName = m_Backup._ListName;
            if (!m_BackupLists.NullOrEmpty())
            {
                int index = 1;
                while (m_BackupLists.Select(backupList => backupList._ListName).Contains(newName))
                {
                    newName = $"{m_Backup._ListName} ({index})";
                    index++;
                }
            }
            m_Backup._ListName = newName;

            m_BackupLists.Add(m_Backup);

            base.OnAcceptKeyPressed();
        }
    }

    public class Dialog_LoadList : Window
    {
        private readonly List<ExposableBackupList> m_BackupLists;
        private readonly List<BulkRecipe> m_BulkRecipes;

        private string m_SearchboxBuffer = "";

        private Vector2 m_ScrollPosition = Vector2.zero;
        private float m_ScrollViewHeight = 0f;

        public Dialog_LoadList(List<ExposableBackupList> backupLists, List<BulkRecipe> bulkRecipes)
        {
            forcePause = true;
            doCloseButton = true;
            closeOnAccept = true;
            closeOnCancel = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            m_BackupLists = backupLists;
            m_BulkRecipes = bulkRecipes;
        }

        public override Vector2 InitialSize => new Vector2(550f, 625f);

        private List<ExposableBackupList> GetShowableBackupLists()
        {
            return m_BackupLists
                .Where(backup => m_SearchboxBuffer.NullOrEmpty() || backup._ListName.ToLower().Contains(m_SearchboxBuffer.ToLower()))
                .ToList();
        }

        public override void DoWindowContents(Rect canva)
        {
            Rect searchRect = canva.TopPart(0.8f).TopPart(0.07f);
            Rect backupRect = canva.BottomPart(0.875f).TopPart(0.9f);

            ShowSearchbox(searchRect);
            ShowBackupLists(backupRect);
        }

        private void ShowSearchbox(Rect searchRect)
        {
            Rect searchboxRect = searchRect.RightPart(0.9f).LeftPart(0.95f);
            Rect iconRect = searchRect.LeftPartPixels(32f);

            m_SearchboxBuffer = Widgets.TextField(searchboxRect, m_SearchboxBuffer);
            Widgets.ButtonImage(iconRect, TexButton.Search, Color.white, Color.white, false);
        }

        private void ShowBackupLists(Rect backupRect)
        {
            Rect outRect = new Rect(Vector2.zero, backupRect.size);
            Rect viewRect = new Rect(Vector2.zero, new Vector2(backupRect.width - MYB_Data.GapX, m_ScrollViewHeight));

            GUI.BeginGroup(backupRect);
            Widgets.BeginScrollView(outRect, ref m_ScrollPosition, viewRect, true);

            const float height = 35f;
            float currentHeight = 0f;
            int i = 0;
            foreach (ExposableBackupList backup in GetShowableBackupLists())
            {
                Rect entryRect = new Rect(0f, currentHeight, viewRect.width, height);

                if (i % 2 != 0)
                    Widgets.DrawBoxSolid(entryRect, new Color(1f, 1f, 1f, 0.1f));

                ShowBackupEntry(entryRect, backup);

                currentHeight += height + MYB_Data.GapY * 2f;
                i++;
            }
            m_ScrollViewHeight = currentHeight;

            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private void ShowBackupEntry(Rect entryRect, ExposableBackupList backup)
        {
            Rect rightRect = entryRect.RightPart(0.325f);

            Rect labelRect = entryRect.LeftHalf().RightPart(0.95f).BottomPart(0.85f);
            Rect addRect = rightRect.LeftPart(0.75f).LeftHalf();
            Rect loadRect = rightRect.LeftPart(0.8f).RightHalf();
            Rect deleteRect = rightRect.RightPartPixels(30f);

            Widgets.Label(labelRect, backup._ListName);

            if (Widgets.ButtonText(addRect, MYB_Data.AddList_Button))
            {
                m_BulkRecipes.AddRange(backup._BulkRecipes);
                base.Close();
            }
            if (Widgets.ButtonText(loadRect, MYB_Data.LoadList_Button))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                m_BulkRecipes.Clear();
                m_BulkRecipes.AddRange(backup._BulkRecipes);
                base.Close();
            }
            if (Widgets.ButtonImage(deleteRect, TexButton.Delete))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                Find.WindowStack.Add(new Dialog_MessageBox(
                    MYB_Data.DeleteListDialog_Message(backup._ListName),
                    MYB_Data.Confirm_Button, () => m_BackupLists.Remove(backup),
                    MYB_Data.Cancel_Button, null, MYB_Data.DeleteListDialog_Title(backup._ListName),
                    true));
            }
        }
    }
}
