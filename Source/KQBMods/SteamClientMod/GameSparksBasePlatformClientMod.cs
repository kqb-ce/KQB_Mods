using GameLogic;
using HarmonyLib;
using LiquidBit.KillerQueenX;
using System;
using System.Collections.Generic;

namespace SteamClientMod
{
	// TODO find some way to not have to do this. when update calls SetStatus it always uses the base class method instead
	[HarmonyPatch(typeof(StatusManager))]
	[HarmonyPatch("Update")]
	public static class Update_Patch
	{
		public static bool Prefix(StatusManager __instance, IPlatformClient ___platformClient, bool ___needsStatusUpdate, bool ___updateStatusInProgress, GameLogic.Profile.Status ___profileStatus,
								  List<Action<bool>> ___updateStatusCallbacks)
		{
			if (!___needsStatusUpdate)
			{
				return false;
			}
			___updateStatusInProgress = true;
			___needsStatusUpdate = false;
			SteamClientNoGS client = (SteamClientNoGS)___platformClient;
			client.NoGSSetStatus(___profileStatus, __instance.allowFriendsToJoinParty, __instance.allowFriendsOfFriendsToJoinParty, __instance.allowFriendsToJoinCustomMatch, __instance.allowSpectateCustomMatch, delegate (bool success)
			{
				foreach (Action<bool> updateStatusCallback in ___updateStatusCallbacks)
				{
					updateStatusCallback(success);
				}
				___updateStatusCallbacks.Clear();
				___updateStatusInProgress = false;
			});
			return false;
		} 
	}

	[HarmonyPatch(typeof(GameSparksBasePlatformClient))]
	[HarmonyPatch("SetPartyStatus")]
	public static class SetPartyStatus_Patch 
	{
		public static bool Prefix(GameSparksBasePlatformClient __instance, bool inRemoteParty, bool partyLeader, int partyMemberCount, int localPlayerCount)
		{
			SteamClientNoGS client = (SteamClientNoGS)__instance;
			client.NoGSSetPartyStatus(inRemoteParty, partyLeader, partyMemberCount, localPlayerCount);
			return false;
		}
	}
}
