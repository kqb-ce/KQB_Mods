using HarmonyLib;
using System;
using UnityEngine;

namespace FixSnail
{

    [HarmonyPatch(typeof(PlayerVisuals), nameof(PlayerVisuals.HighestPlayerSortingOrder))]
    public static class MyPrefixPatch
    {
        static bool Prefix(ref int __result)
        {
            __result = 2100;
            return false;      
        }
    }
}
