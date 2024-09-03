using HarmonyLib;
using LiquidBit.KillerQueenX.Utility;
using System.Collections.Generic;
using System;
using static CustomMatchManager;
using GameLogic;
using LiquidBit.KillerQueenX;
using System.Linq;
using UnityEngine;

namespace ProfileCacheMod
{
    [HarmonyPatch(typeof(LiquidBit.KillerQueenX.GameSparksBasePlatformClient))]
    [HarmonyPatch("AssignProfile")]
    public static class Base_Patch
    {
        static string apiKey = "";
        static void Postfix(LiquidBit.KillerQueenX.GameSparksBasePlatformClient __instance, GameLogic.Profile profile)
        {
            Debug.Log("Assigning Profile");
            Debug.Log(__instance.GetAccount());
            Debug.Log(__instance.GetAccount().displayName);
            Debug.Log(__instance.GetAccount().avatarUrl);
        }
    }

    [HarmonyPatch(typeof(CustomMatchManager))]
    [HarmonyPatch("AddPartyToCustomMatch")]
    public static class Valid_Patch
    {
        public static bool Prefix(Party party, CustomMatchActor.Type actorType, CustomMatchActor.State initialState = CustomMatchActor.State.AddToServer, float timeout = 0f)
        {
            UnityEngine.Debug.Log("Adding party to custom match...\n" + party.partyMembers.ElementAt(0).displayName);

            return true;
        }

    }

}
