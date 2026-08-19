using HarmonyLib;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.Collections.Generic;

using static PostMatchUI_Results_PlayerStats;

namespace RestoreMainMenu
{
    [HarmonyPatch(typeof(MainMenuVideoPlayer))]
    [HarmonyPatch("Start")]
    public static class RemoveVideo_Patch
    {
        public static bool Prefix(MainMenuVideoPlayer __instance)
        {
            Transform parent = __instance.transform.parent;
            UnityEngine.UI.Image img = parent.GetComponentInChildren<UnityEngine.UI.Image>();
            Debug.Log(img);

            img.sprite = GameManager.GMInstance.assetSystem.LoadAsset<Sprite>("Title-screen-Premenu_BG", "Assets/Sprites/UI/Background/Title-screen-Premenu_BG.png");


            return false;
        }

    }

    [HarmonyPatch(typeof(IntroMenu))]
    [HarmonyPatch("toStartSequence")]
    public static class IntroMenu_Patch
    {
        public static Sprite OldSplashSprite = GameManager.GMInstance.assetSystem.LoadAsset<Sprite>("Title-screen-Premenu_BG", "Assets/Sprites/UI/Background/Title-screen-Premenu_BG.png");
        public static bool Prefix(IntroMenu __instance, ref CanvasGroup ___start)
        {
            UnityEngine.UI.Image img = ___start.gameObject.GetComponentInChildren<UnityEngine.UI.Image>();
            Debug.Log(img);

            img.sprite = OldSplashSprite;


            return true;
        }

    }

    [HarmonyPatch(typeof(UI_Overlay))]
    [HarmonyPatch("Awake")]
    public static class Awake_Patch
    {
        public static bool Prefix(UI_Overlay __instance, ref CanvasGroup ___canvasGroup)
        {
            UnityEngine.UI.Image[] imgs =___canvasGroup.GetComponentsInChildren<UnityEngine.UI.Image>(true);
            foreach(UnityEngine.UI.Image img in imgs)
            {
                if (img.GetName() == "Title")
                {
                    img.sprite = GameManager.GMInstance.assetSystem.LoadAsset<Sprite>("Title-screen-Premenu_BG", "Assets/Sprites/UI/Background/Title-screen-Premenu_BG.png");
                }
            }


            return true;
        }

    }
}
