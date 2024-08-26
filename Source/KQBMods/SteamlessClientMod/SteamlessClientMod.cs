using HarmonyLib;
using LiquidBit.KillerQueenX;
using System.Reflection;
using UnityEngine;

namespace SteamlessClientMod
{
    [HarmonyPatch(typeof(MockClient))]
    [HarmonyPatch("ValidateOwnership")]
    public static class Valid_Patch
    {
        public static bool Prefix(MockClient __instance)
        {
            __instance.GetType().GetField("steamInitialized", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, true);
            return true;
        }

    }

}
