using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace MoreConfetti
{
    [BepInPlugin(myGUID, pluginName, versionString)]
    public class MoreConfettiPlugin : BaseUnityPlugin
    {
        private const string myGUID = "com.treebones.moreconfetti";
        private const string pluginName = "More Confetti Mod";
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
