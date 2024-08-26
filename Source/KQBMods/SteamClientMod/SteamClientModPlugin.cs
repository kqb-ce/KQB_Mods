using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SteamClientMod
{
    [BepInPlugin(myGUID, pluginName, versionString)]
    public class SteamClientModPlugin : BaseUnityPlugin
    {
        private const string myGUID = "com.treebones.steamclientmod";
        private const string pluginName = "Steam Client Mod";
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
