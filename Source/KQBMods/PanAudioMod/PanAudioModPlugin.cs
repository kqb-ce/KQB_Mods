using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace PanAudioMod
{
    [BepInPlugin(myGUID, pluginName, versionString)]
    public class PanAudioModPlugin : BaseUnityPlugin
    {
        private const string myGUID = "com.treebones.pan-audio-mod";
        private const string pluginName = "Pan Audio Mod";
        private const string versionString = "1.0.0";

        private static readonly Harmony harmony = new Harmony(myGUID);

        public static ManualLogSource logger;

        private void Awake()
        {
            harmony.PatchAll();
            Logger.LogInfo(pluginName + " " + versionString + " " + "loaded.");
            logger = Logger;
        }
    }
}
