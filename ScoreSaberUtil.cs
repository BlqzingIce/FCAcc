using System.Reflection;

namespace FCAcc
{
    public static class ScoreSaberUtil
    {
        static MethodBase SS_playbackEnabled = null;
        public static bool isInReplay = false;

        public static bool GetMethodBase()
        {
            return (SS_playbackEnabled =
                IPA.Loader.PluginManager.GetPluginFromId("ScoreSaber")?
                .Assembly.GetType("ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted")?
                .GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic)) != null;
        }

        public static bool UpdateReplayStatus()
        {
            try
            {
                return isInReplay = (SS_playbackEnabled != null && !(bool)SS_playbackEnabled.Invoke(null, null));
            }
            catch { }
            return isInReplay = false;
        }

        public static void ResetReplayStatus()
        {
            isInReplay = false;
        }
    }
}
