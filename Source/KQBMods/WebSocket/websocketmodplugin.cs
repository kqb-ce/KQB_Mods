using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace WebSocket
{
    [BepInPlugin(myGUID, pluginName, versionString)]
    public class websocketmodplugin : BaseUnityPlugin
    {
        private const string myGUID = "com.treebones.websocketmod";
        private const string pluginName = "Web Socket Mod";
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
