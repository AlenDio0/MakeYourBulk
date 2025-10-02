using UnityEngine;
using Verse;

namespace MakeYourBulk
{
    public class MakeYourBulkMod : Mod
    {
        public static MakeYourBulkSettings s_Settings;
        public static ModContentPack s_ModContent;

        public MakeYourBulkMod(ModContentPack content)
            : base(content)
        {
            s_Settings = GetSettings<MakeYourBulkSettings>();
            s_ModContent = content;
            LongEventHandler.ExecuteWhenFinished(s_Settings.AddToDatabase);
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            if (s_Settings == null)
            {
                MYB_Log.Error("s_Settings is null.");
                return;
            }

            s_Settings.DoWindowContents(inRect);

            base.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();

            s_Settings.AddToDatabase();
        }

        public override string SettingsCategory() => MYB_Data.SpacedModName;
    }
}
