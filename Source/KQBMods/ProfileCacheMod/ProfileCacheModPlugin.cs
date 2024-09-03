using BepInEx.Logging;
using BepInEx;
using HarmonyLib;

namespace ProfileCacheMod
{
    [BepInPlugin(myGUID, pluginName, versionString)]
    public class ProfileCacheModPlugin : BaseUnityPlugin
    {
        private const string myGUID = "com.treebones.profilecachemod";
        private const string pluginName = "Profile Cache Mod";
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
