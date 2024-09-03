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
			SteamClientNoGS client = new SteamClientNoGS(___steamManagerPrefab);
			client.Init();
			__result = client;
			return false;
		}
	}
	/**
	[HarmonyPatch(typeof(LB_AchievementManager))]
	[HarmonyPatch("PullAchievementsGameSparks")]
	public static class PullAchievementsGameSparks_Patch
	{
		public static bool Prefix(Action onSuccess = null, Action onFailure = null)
			//TODO
		{/**
			LB_AchievementManager achievementManager = GameManager.GMInstance.achievementManager;
			if (___AchievementLookupTable == null)
			{
				___AchievementLookupTable = new AchievementLookup();
				___AchievementLookupTable.AddPlatformIds(___gameSparksPlatformIdLookup, AchievementPlatform.GameSparks);
				if (__instance.AdditionalUnlockPlatform != 0)
				{
					___AchievementLookupTable.AddPlatformIds(___platformIdLookup, __instance.AdditionalUnlockPlatform);
					if (___platformStatInfoLookup != null && ___platformStatInfoLookup.Count > 0)
					{
						___AchievementLookupTable.AddStatInfo(___platformStatInfoLookup, __instance.AdditionalUnlockPlatform);
					}
				}
			}
			//GameManager.GMInstance.achievementManager.GetType().GetField("isFinishedGsPull", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(GameManager.GMInstance.achievementManager, true);
			onSuccess?.Invoke();
			return false;
		}

	}**/

	public class SteamClientNoGS : GameSparksBasePlatformClient
	{
		static string apiKey = "58A1C35D4080216609E6862A2C348DCD";

		class GetPlayerSummaries
		{
			public GetPlayerSummariesResponse response;
		}

		public class GetPlayerSummariesResponse
		{
			public List<SteamPlayerSummary> players;
		}

		public class SteamPlayerSummary
		{
			public string steamid;
			public int communityvisibilitystate;
			public int profilestate;
			public string personaname;
			public string profileurl;
			public string avatar;
			public string avatarmedium;
			public string avatarfull;
			public string avatarhash;
			public int personastate;
		}

		protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;

		private bool initialized;

		private GameObject steamManagerPrefab;

		private string steamAuthTicket;

		public SteamClientNoGS(GameObject steamManagerPrefab)
		{
			this.steamManagerPrefab = steamManagerPrefab;
		}

		public override void Init()
		{
			Debug.Log("---SteamClientNoGS: STEAM CLIENT INIT CALLED");
			bool flag = SteamAPI.Init();
			if (!initialized)
			{
				if (flag)
				{
					UnityEngine.Object.Instantiate(steamManagerPrefab);
					Debug.Log("----SteamClientNoGS: STEAM CLIENT INITIALIZED");
					initialized = true;
					discordUtility = new DiscordUtility(CreateFlags.NoRequireDiscord);
					m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
					if (SteamUtils.IsSteamInBigPictureMode())
					{
						GameManager.GMInstance.ForceFullScreen();
					}
				}
				else
				{
					Debug.Log("--- STEAM CLIENT INIT STEAM NOT RUNNING");
				}
			}
			else
			{
				Debug.Log("--- STEAM CLIENT INIT ALREADY INITIALIZED");
			}
		}

		public override void Update()
		{
			if (initialized)
			{
				discordUtility.Update();
			}
		}

		public override IEnumerator ValidateOwnership(Action valid, Action invalid)
		{
			bool flag = false;
			if (initialized)
			{
				flag = true;
			}
			if (flag)
			{
				valid();
			}
			else
			{
				invalid();
			}
			yield break;
		}

		public override IEnumerator DoLoginFlow(bool isStartup, Action onSuccess, Action onFailure, bool onlineServicePrompt)
		{
			Debug.Log("initialized: \n" + initialized + "\nsteam is running: " + SteamAPI.IsSteamRunning());
			try
			{
				//GS.Instance.Reconnect();
			}
			catch (NullReferenceException ex)
			{
				Debug.LogWarning("Got NRE from GS.Instance.Reconnect: " + ex.StackTrace);
				GS.Instance.Reset();
			}
			if (initialized && SteamAPI.IsSteamRunning())
			{
				SteamAuth(onSuccess, onFailure);
			}
			yield return null;
		}

		private void SteamAuth(Action onSuccess, Action onFailure)
		{
			Debug.Log("SteamClientNoGS: DOING STEAM AUTH");
			UpdateAuthStatus(AuthStatus.WaitingForAuth);
			appId = SteamUtils.GetAppID().ToString();
			discordUtility.RegisterSteam(appId);
			if (steamAuthTicket == null)
			{
				byte[] array = new byte[1024];
				SteamUser.GetAuthSessionTicket(array, 1024, out var pcbTicket);
				steamAuthTicket = "";
				for (int i = 0; i < pcbTicket; i++)
				{
					steamAuthTicket += $"{array[i]:X2}";
				}
			}
			Debug.Log("---- steamAuthTicket: " + steamAuthTicket + " length: " + steamAuthTicket.Length);
			GameManager.GMInstance.StartCoroutine(GetAvatarAndLogin(onSuccess, onFailure));
		}

		private IEnumerator GetAvatarAndLogin(Action onSuccess, Action onFailure)
		{
			Debug.Log("--- START GET AVATAR AND LOGIN");
			string steamId = SteamUser.GetSteamID().ToString();
			UnityWebRequestAsyncOperation async = new UnityWebRequest("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + apiKey + "&steamids=" + steamId)
			{
				method = "GET",
				downloadHandler = ((DownloadHandler)new DownloadHandlerBuffer())
			}.SendWebRequest();
			yield return (object)async;
			UnityWebRequest webRequest = async.webRequest;
			if (!webRequest.isNetworkError && !webRequest.isHttpError)
			{
				GetPlayerSummaries getPlayerSummaries = JsonConvert.DeserializeObject<GetPlayerSummaries>(webRequest.downloadHandler.text);
				if (getPlayerSummaries.response.players.Count > 0)
				{
					SteamPlayerSummary summary = getPlayerSummaries.response.players[0];

					Client client = NakamaUtils.GetClient();
					Task<ISession> sessionTask = client.AuthenticateSteamAsync(steamAuthTicket);
					yield return new WaitUntil(() => sessionTask.IsCompleted);

					if (sessionTask.Exception != null)
					{
						Debug.LogWarning("Nakama: SteamConnectRequest failed - steamTicket : " + sessionTask.Exception);
						steamAuthTicket = null;
						UpdateAuthStatus(AuthStatus.NotAuthed);
						onFailure();
					}
					else {
						PlayerPrefs.SetString("nakama.authToken", sessionTask.Result.AuthToken);
						PlayerPrefs.SetString("nakama.refreshToken", sessionTask.Result.RefreshToken);

						//TODO not sure about this for liquid id
						sessionTask.Result.Vars.Add("liquidId", sessionTask.Result.Username);
						sessionTask.Result.Vars.Add("avatarUrl", summary.avatarfull);
						sessionTask.Result.Vars.Add("displayName", summary.personaname);


						List<IDictionary<string, string>> listDataDict = new List<IDictionary<string, string>>();
						listDataDict.Add(sessionTask.Result.Vars);
						GameLogic.Profile profile = new NakamaUtils().ConvertUserToProfile(sessionTask.Result, steamId);

						SuccessfulLogin(sessionTask.Result.AuthToken, profile);
						CallOnUserIdUpdated(steamId);
						onSuccess();
						Debug.LogFormat("Found account for user: {0}", sessionTask.Result.UserId);
					}
				}
			}
		}

		public override void RefreshFriendsList(Action successCallback, Action errorCallback)
		{
			int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
			List<string> list = new List<string>();
			for (int i = 0; i < friendCount; i++)
			{
				list.Add(SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate).ToString());
			}
			base.FetchProfiles("steamIds", list, includeRank: true, friendsList: true, delegate (GameLogic.Profile profile)
			{
				UpdateFriendFromProfile(profile);
			}, delegate (List<GameLogic.Profile> profiles)
			{
				friends = profiles;
				FriendsListUpdated(friends);
				if (successCallback != null)
				{
					successCallback();
				}
			}, errorCallback);
		}

		public override void Dispose()
		{
			discordUtility.Dispose();
		}

		private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
		{
			if (pCallback.m_bActive != 0)
			{
				Debug.Log("Steam Overlay has been activated");
				GameManager.GMInstance.inputManager.DisableAllInput();
			}
			else
			{
				Debug.Log("Steam Overlay has been closed");
				GameManager.GMInstance.StartCoroutine(DelayedSetLastControllerScheme());
			}
		}

		private IEnumerator DelayedSetLastControllerScheme()
		{
			yield return new WaitForSecondsRealtime(0.1f);
			GameManager.GMInstance.inputManager.SetLastControllerScheme();
		}

		public override Platform GetPlatformID()
		{
			return Platform.Steam;
		}

		public void NoGSSetStatus(GameLogic.Profile.Status status, bool allowFriendsToJoinParty, bool allowFriendsOfFriendsToJoinParty, bool allowFriendsToJoinCustomMatch, bool allowSpectateCustomMatch, Action<bool> callback = null)
		{
			if (account == null)
			{
				return;
			}
			if (account.status != status || account.allowFriendsToJoinParty != allowFriendsToJoinParty || account.allowFriendsOfFriendsToJoinParty != allowFriendsOfFriendsToJoinParty || account.allowFriendsToJoinCustomMatch != allowFriendsToJoinCustomMatch || account.allowSpectateCustomMatch != allowSpectateCustomMatch || account.currentNetworkingPreferences != (int)GetPlayerNetworkingPreferences())
			{
				Dictionary<string, string> statusProperties = new Dictionary<string, string>();
				statusProperties.Add("status", status.ToString());
				statusProperties.Add("allowFriendsToJoinParty", allowFriendsToJoinParty ? "true" : "false");
				statusProperties.Add("allowFriendsOfFriendsToJoinParty", allowFriendsOfFriendsToJoinParty ? "true" : "false");
				statusProperties.Add("allowFriendsToJoinCustomMatch", allowFriendsToJoinCustomMatch ? "true" : "false");
				statusProperties.Add("allowSpectateCustomMatch", allowSpectateCustomMatch ? "true" : "false");
				statusProperties.Add("crossPlayEnabled", ((int)GetPlayerNetworkingPreferences()).ToString());

				GameManager.GMInstance.StartCoroutine(SetStatusIEnum(statusProperties, status, allowFriendsToJoinParty, allowFriendsOfFriendsToJoinParty, allowFriendsToJoinCustomMatch, allowSpectateCustomMatch, callback));

			}
			else if (callback != null)
			{
				callback(obj: true);
			}
		}
		public IEnumerator SetStatusIEnum(Dictionary<string, string> statusProperties, GameLogic.Profile.Status status, bool allowFriendsToJoinParty, bool allowFriendsOfFriendsToJoinParty, bool allowFriendsToJoinCustomMatch,
											bool allowSpectateCustomMatch, Action<bool> callback = null)
		{
			Task statusTask = NakamaUtils.GetClient().EventAsync(NakamaUtils.RestoreSession(), "setInGameStatus", statusProperties, new RetryConfiguration(1000, 1));

            yield return new WaitUntil(() => statusTask.IsCompleted);
			if (statusTask.Exception != null)
			{
				Debug.Log("---- Steam CLient NO GS: Error updating in game status ---- " + statusTask.Exception);
				if (callback != null)
				{
					callback(obj: false);
				}
			}
			else
			{
				Debug.Log(string.Concat("---- Updated Profile Status To ", status, "  ----"));
				if (account != null)
				{
					account.status = status;
					account.allowFriendsToJoinParty = allowFriendsToJoinParty;
					account.allowFriendsOfFriendsToJoinParty = allowFriendsOfFriendsToJoinParty;
					account.allowFriendsToJoinCustomMatch = allowFriendsToJoinCustomMatch;
					account.allowSpectateCustomMatch = allowSpectateCustomMatch;
					account.currentNetworkingPreferences = (int)GetPlayerNetworkingPreferences();
				}
				if (discordUtility != null)
				{
					discordUtility.UpdateActivity(status, this);
				}
				if (callback != null)
				{
					callback(obj: true);
				}
				UpdatePlatformStatus(status);
			}
		}

		public void NoGSSetPartyStatus(bool inRemoteParty, bool partyLeader, int partyMemberCount, int localPlayerCount)
		{
			if (account == null || authStatus != AuthStatus.Authed || (inRemoteParty == account.inParty && partyLeader == account.partyLeader && partyMemberCount == account.partyCount && localPlayerCount == account.localPlayerCount))
			{
				return;
			}
			if (inRemoteParty && partyMemberCount >= 2)
			{
				// TODO achievements?
				// GameManager.GMInstance.achievementManager.PushAchievement(AchievementName.JoinPartyOrInviteFriend);
			}
			PartyStatusUpdated();

			Dictionary<string, string> partyProperties = new Dictionary<string, string>();
			partyProperties.Add("inRemoteParty", inRemoteParty ? "true" : "false");
			partyProperties.Add("partyLeader", partyLeader ? "true" : "false");
			partyProperties.Add("count", partyMemberCount.ToString());
			partyProperties.Add("localPlayerCount", localPlayerCount.ToString());

			GameManager.GMInstance.StartCoroutine(SetPartyStatusIEnum(partyProperties, inRemoteParty, partyLeader, partyMemberCount, localPlayerCount));
		}

		public IEnumerator SetPartyStatusIEnum(Dictionary<string, string> partyProperties, bool inRemoteParty, bool partyLeader, int partyMemberCount, int localPlayerCount)
		{
			Task partyStatusTask = NakamaUtils.GetClient().EventAsync(NakamaUtils.RestoreSession(), "setPartyMemberCount", partyProperties);

			yield return new WaitUntil(() => partyStatusTask.IsCompleted);
			if (partyStatusTask.Exception != null)
			{
				Debug.Log("---- Steam CLient NO GS: Error updating in game status ---- " + partyStatusTask.Exception);
			}
			else
			{
				account.inParty = inRemoteParty;
				account.partyLeader = partyLeader;
				account.partyCount = partyMemberCount;
				account.localPlayerCount = localPlayerCount;
				if (discordUtility != null)
				{
					discordUtility.UpdateActivity(account.status, this);
				}
				Debug.Log("---- SteamClient NOGS: Updated Party member count to " + partyMemberCount + "  ----");
			}
		}
	}

}
