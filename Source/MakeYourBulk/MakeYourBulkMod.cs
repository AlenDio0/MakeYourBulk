using HarmonyLib;
using UnityEngine;
using Verse;

namespace MakeYourBulk
{
    public class MakeYourBulkMod : Mod
    {
        public static MakeYourBulkSettings settings;

        public MakeYourBulkMod(ModContentPack content)
            : base(content)
        {
            settings = GetSettings<MakeYourBulkSettings>();
            LongEventHandler.ExecuteWhenFinished(settings.AddToDatabase);
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            if (settings == null)
            {
                MYB_Log.Error("settings is null.");
                return;
            }

            settings.DoWindowContents(inRect);

            base.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();

            settings.RemoveFromDatabase();
            settings.AddToDatabase();
        }

        public override string SettingsCategory() => MYB_Data.SpacedModName;
    }

    [StaticConstructorOnStartup]
    public static class MakeYourBulk
    {
        static MakeYourBulk()
        {
            Harmony harmony = new Harmony("com.alendio.makeyourbulk");
            harmony.PatchAll();
        }
    }
}
