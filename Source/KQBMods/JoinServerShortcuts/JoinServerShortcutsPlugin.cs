using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace LosersCanDanceMod
{
    [BepInPlugin(myGUID, pluginName, versionString)]
    public class JoinServerShortcutsPlugin : BaseUnityPlugin
    {
        private const string myGUID = "com.treebones.JoinServerShortcuts";
        private const string pluginName = "JoinServer Shortcuts";
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
