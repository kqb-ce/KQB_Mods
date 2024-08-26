using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace LosersCanDanceMod
{
    [BepInPlugin(myGUID, pluginName, versionString)]
    public class SteamlessClientModPlugin : BaseUnityPlugin
    {
        private const string myGUID = "com.treebones.steamlessclientmod";
        private const string pluginName = "Steamless Client Mod";
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
