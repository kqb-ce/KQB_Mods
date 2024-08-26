using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SteamlessClientMod
{
    [BepInPlugin(myGUID, pluginName, versionString)]
    public class SteamlessClientModPlugin : BaseUnityPlugin
    {
        private const string myGUID = "com.treebones.loserscandancemod";
        private const string pluginName = "Losers Can Dance Mod";
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
