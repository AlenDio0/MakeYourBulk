namespace MakeYourBulk
{
    public static class MYB_Log
    {
        private static string Format(string message) => $"{MYB_Data.ModName}: {message}.";

        public static void Trace(string message)
        {
            if (MakeYourBulkMod.s_Settings.VerboseLogging)
            {
                Verse.Log.Message(Format(message));
            }
        }
        public static void Warn(string message) => Verse.Log.Warning(Format(message));
        public static void Error(string message) => Verse.Log.Error(Format(message));
    }
}
