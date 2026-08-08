using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine.SceneManagement;

namespace ServerOnly;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private static readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        Logger = base.Logger;

        string[] commandLineArgs = Environment.GetCommandLineArgs();
        for (int i = 1; i < commandLineArgs.Length; i++)
        {
            UnityEngine.Debug.Log(commandLineArgs[i]);
            if (commandLineArgs[i] == "-batchmode")
            {
                Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
                harmony.PatchAll();
                break;
            }
        }

    }
}
