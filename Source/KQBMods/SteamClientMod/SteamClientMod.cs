using HarmonyLib;
using Steamworks;
using System;
using System.Collections;
using UnityEngine.Networking;
using BepInEx.Logging;
using GameSparks.Core;
using Newtonsoft.Json;
using LiquidBit.KillerQueenX;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using GameLogic;
using LiquidBit.KillerQueenX.Utility;
using Discord;
using GameLogic.Utility;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using Nakama;
using System.Threading.Tasks;

namespace SteamClientMod
{
	[HarmonyPatch(typeof(GameManager))]
	[HarmonyPatch("CreatePlatformClient")]
	public static class CreatePlatformClient_Patch
	{
		public static bool Prefix(ref IPlatformClient __result, GameObject ___steamManagerPrefab)
		{
            LiquidBit.KillerQueenX.SteamClient client = new LiquidBit.KillerQueenX.SteamClient(___steamManagerPrefab);
			client.Init();
			__result = client;
			return false;
		}
	}

	[HarmonyPatch(typeof(LiquidBit.KillerQueenX.GameSparksBasePlatformClient))]
	[HarmonyPatch("AddAuthHeaders")]
	public static class AddAuthHeaders_Patch
	{
		public static bool Prefix(Dictionary<string,string> headers)
		{
			return false;
		}
	}
}
