using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace KQB_o_ween
{
    [BepInPlugin(myGUID, pluginName, versionString)]
    public class HalloweenPlugin : BaseUnityPlugin
    {
        private const string myGUID = "com.treebones.halloween";
        private const string pluginName = "Halloween";
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
