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
        public static bool Prefix(bool ___steamInitialized)
        {
            ___steamInitialized = true;
            return true;
        }

    }

}
