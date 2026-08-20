using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RestoreMainMenu;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private static readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
    public static Sprite CustomSprite { get; set; }
    private void Awake()
    {
        // If a `Bepinex/Plugins/images/` directory exists, randomly choose an image from it for the background
        LoadCustomBgImage();

        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        harmony.PatchAll();

    }

    void LoadCustomBgImage()
    {

        if (Directory.Exists(Paths.PluginPath + "/images/"))
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var bgFiles = Directory.EnumerateFiles(Paths.PluginPath + "/images/", "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => allowedExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase));
            if (bgFiles.Count() > 0)
            {
                System.Random rand = new System.Random();

                string randomFile = bgFiles.ElementAt<string>(rand.Next(bgFiles.Count()));
                byte[] fileData = File.ReadAllBytes(randomFile);
                Texture2D tex = new Texture2D(2, 2);
                if (tex.LoadImage(fileData))
                {
                    CustomSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }
        }

    }
}
