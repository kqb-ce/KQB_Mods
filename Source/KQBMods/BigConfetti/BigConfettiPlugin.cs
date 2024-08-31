using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace BigConfetti
{
    [BepInPlugin(myGUID, pluginName, versionString)]
    public class BigConfettiPlugin : BaseUnityPlugin
    {
        private const string myGUID = "com.treebones.BigConfetti";
        private const string pluginName = "Big Confetti";
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
