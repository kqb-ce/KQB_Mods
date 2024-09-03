using GameLogic;
using HarmonyLib;
using LiquidBit.KillerQueenX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static MatchFinder;

namespace fixQP
{
	[HarmonyPatch(typeof(MatchmakerClient), "StartMatchmaking")]
	public class Patches
	{
		class SimpleEnumerator : IEnumerable
		{
			public IEnumerator enumerator;
			public Action prefixAction, postfixAction;
			public Action<object> preItemAction, postItemAction;
			public Func<object, object> itemAction;
			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
			public IEnumerator GetEnumerator(List<GameLogic.WebService.Model.Player> players, MatchType matchType, string partyId, MatchmakerClient mmClient)
			{
				prefixAction();
				enumerator.MoveNext();
				//while (enumerator.MoveNext())
				{
					enumerator.MoveNext();
					var item = enumerator.Current;
					preItemAction(item);
					yield return itemAction(item);
					postItemAction(item);
				}
				postfixAction();
			}
		}
		static void Postfix(MatchmakerClient __instance, ref IEnumerator __result, List<GameLogic.WebService.Model.Player> players, MatchType matchType, string partyId)
		{
			Action prefixAction = () => { Console.WriteLine("--> beginning"); };
			Action postfixAction = () => { Console.WriteLine("--> ending"); };
			Action<object> preItemAction = (item) => { Console.WriteLine($"--> before {item}"); };
			Action<object> postItemAction = (item) => { Console.WriteLine($"--> after {item}"); };
			Func<object, object> itemAction = (item) =>
			{
				var newItem = item + "+";
				Console.WriteLine($"--> item {item} => {newItem}");
				return newItem;
			};
			var myEnumerator = new SimpleEnumerator()
			{
				enumerator = __result,
				prefixAction = prefixAction,
				postfixAction = postfixAction,
				preItemAction = preItemAction,
				postItemAction = postItemAction,
				itemAction = itemAction
			};
			__result = myEnumerator.GetEnumerator(players, matchType, partyId, __instance);
		}

	}
}
	/**
    [HarmonyPatch(typeof(MatchFinder))]
    [HarmonyPatch("StartMatchmaking")]
    public static class StartMatchmaking_Patch
    {
        public static bool Prefix(MatchFinder __instance, MatchType matchType, JoinedServerError ___OnJoinedServerFailure)
        {
            __instance.StartCoroutine(new MMUtils().StartMatchmakingCoroutine(matchType, __instance, ___OnJoinedServerFailure));
            __instance.GetType().GetField("steamInitialized", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, true);
            return false;
        }

    }

    public class MMUtils
    {
        public IEnumerator StartMatchmakingCoroutine(MatchType matchType, MatchFinder matchFinder, JoinedServerError OnJoinedServerFailure) 
        {

            yield return StartMatchmaking(matchFinder.GetPlayersForMatch(), matchType, matchFinder.gameManager.partyManager.GetParty().teamId);
            if (matchFinder.matchmakerClient.state == MatchmakerClient.State.Searching)
            {
				matchFinder.gameManager.partyManager.NotifyPartyStartedMatchmaking(matchFinder.matchmakerClient.estimatedWaitTimeInSeconds);
                yield return matchFinder.matchmakerClient.PollMatchmakingStatus(matchType);
                if (matchFinder.matchmakerClient.lastError == MatchmakerClient.Error.None)
                {
					matchFinder.gameManager.partyManager.NotifyPartyOfTicketIdToConnectTo(matchFinder.matchmakerClient.ticketId, matchType);
                    yield return matchFinder.ConnectToMatchWithTicketId(matchFinder.matchmakerClient.ticketId, matchFinder.GetPlayersForConnection(), matchType);
                }
                else if (matchFinder.matchmakerClient.lastError != MatchmakerClient.Error.Cancelled)
                {
					OnJoinedServerFailure(matchFinder.matchmakerClient.lastError);
                }
            }
            else if (matchFinder.matchmakerClient.lastError != 0)
            {
				OnJoinedServerFailure(matchFinder.matchmakerClient.lastError);
            }
        }

		public IEnumerator StartMatchmaking(List<GameLogic.WebService.Model.Player> players, MatchType matchType, string partyId)
		{
			Reset(Error.None);
			running = true;
			this.players = players;
			this.matchType = matchType;
			this.partyId = partyId;
			Debug.Log("Starting matchmaking with player ids: ");
			foreach (GameLogic.WebService.Model.Player player in players)
			{
				Debug.Log(player.playerId);
			}
			SetState(State.SentRequest);
			IPlatformClient platformClient = GameManager.GMInstance.platformClient;
			string jsonBody = JsonConvert.SerializeObject(new StartMatchmaking.Request
			{
				players = players,
				matchType = matchType,
				partyId = partyId,
				platformId = platformClient.GetPlatformID(),
				crossplayEnabled = GameManager.GMInstance.partyManager.PartyAllowsCrossplay(),
				preferredRegions = RegionManager.GetPreferredRegionKeys()
			});
			Debug.Log("---- GS ENV: " + EnvironmentConfig.Instance.environment);
			yield return KQBWebServiceClient.MakeRequest("startmatchmaking", jsonBody, delegate (UnityWebRequest webRequest)
			{
				if (state == State.SentRequest)
				{
					string text = webRequest.downloadHandler.text;
					if (!webRequest.isNetworkError && !webRequest.isHttpError)
					{
						StartMatchmaking.Response response = JsonConvert.DeserializeObject<StartMatchmaking.Response>(text);
						if (response != null)
						{
							switch (response.status)
							{
								case GameLogic.WebService.Model.StartMatchmaking.Status.Success:
									Debug.Log("GameLift Ticket id: " + response.ticketId);
									ticketId = response.ticketId;
									estimatedWaitTimeInSeconds = response.estimatedWaitTimeInSeconds;
									SetState(State.Searching);
									break;
								case GameLogic.WebService.Model.StartMatchmaking.Status.InTimeout:
									{
										for (int i = 0; i < response.profilesInTimeout.Count; i++)
										{
											GameLogic.Profile profile = response.profilesInTimeout[i];
											GameManager.GMInstance.profileCache.AddProfileToCache(profile);
										}
										Reset(Error.InRankedTimeout);
										break;
									}
								case GameLogic.WebService.Model.StartMatchmaking.Status.ExistingRankedGame:
									Reset(Error.ExistingRankedGame);
									break;
								case GameLogic.WebService.Model.StartMatchmaking.Status.TooManyControllers:
									Reset(Error.TooManyControllers);
									break;
							}
						}
						else
						{
							Reset(Error.WebServiceError);
						}
					}
					else
					{
						if (text == Auth.InvalidClientVersionError)
						{
							Reset(Error.InvalidClientVersion);
						}
						else if (text == Auth.OutsideTimeWindowError)
						{
							Reset(Error.OutsideTimeWindow);
						}
						else
						{
							Reset(Error.WebServiceError);
						}
						Debug.Log(text);
					}
				}
			});
		}**/
