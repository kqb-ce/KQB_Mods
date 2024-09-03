using GameLogic;
using GameLogic.WebService.Model;
using HarmonyLib;
using LiquidBit.KillerQueenX;
using LiquidBit.KillerQueenX.Utility;
using Nakama;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using static MatchFinder;
using static MatchmakerClient;

namespace fixQP
{
	[HarmonyPatch(typeof(MatchmakerClient), "StartMatchmaking")]
	public class Patches
	{
		public class SimpleEnumerator : IEnumerable
		{
			public IEnumerator enumerator;
			public Action prefixAction, postfixAction;
			public Action<object> preItemAction, postItemAction;
			public Func<object, object> itemAction;
			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
			private List<GameLogic.WebService.Model.Player> players;
			private MatchType matchType;
			private string partyId;
			MatchmakerClient mmClient;
			public SimpleEnumerator()
			{
			}
			public SimpleEnumerator(List<GameLogic.WebService.Model.Player> players, MatchType matchType, string partyId, MatchmakerClient mmClient)
			{
				this.players = players;
				this.matchType = matchType;
				this.partyId = partyId;
				this.mmClient = mmClient;
			}
			public IEnumerator GetEnumerator()
			{
				prefixAction();
				while (enumerator.MoveNext())
				{
					var item = enumerator.Current;
					preItemAction(item);
					yield return itemAction(item);
					postItemAction(item);
				}
			}

			public IEnumerator doRealMM(MatchFinder matchFinder)
			{
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

				ISocket sock = Socket.From(SteamClientMod.NakamaUtils.GetClient());
				Task sockTask = sock.ConnectAsync(SteamClientMod.NakamaUtils.RestoreSession());
				Debug.Log("Connecting Socket");
				yield return new WaitUntil(() => sockTask.IsCompleted);
				Debug.Log("Socket Connected");
				Task<IMatchmakerTicket> mmTask = sock.AddMatchmakerAsync("*", 2, 8);
				Debug.Log("match maker started...");
				yield return new WaitUntil(() => mmTask.IsCompleted);
				Debug.Log("MatchMaker returned...");
				Debug.Log(mmTask.Result.Ticket);

				Debug.Log("Ticket id: " + mmTask.Result.Ticket);
				mmClient.ticketId = mmTask.Result.Ticket;
				mmClient.estimatedWaitTimeInSeconds = 0;
				mmClient.state = MatchmakerClient.State.Searching;
				typeof(MatchFinder).GetMethod("MatchmakerClientOnStateUpdate", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(matchFinder, new object[] { new StateUpdateMessage
		{
			state = mmClient.state,
			estimatedWaitTime = mmClient.estimatedWaitTimeInSeconds
		} });


				if (matchFinder.matchmakerClient.state == MatchmakerClient.State.Searching)
				{
					matchFinder.gameManager.partyManager.NotifyPartyStartedMatchmaking(matchFinder.matchmakerClient.estimatedWaitTimeInSeconds);
					yield return matchFinder.matchmakerClient.PollMatchmakingStatus(matchType);
					if (matchFinder.matchmakerClient.lastError == MatchmakerClient.Error.None)
					{
						Debug.Log("GETTING HERE NO ISSUE");
						matchFinder.gameManager.partyManager.NotifyPartyOfTicketIdToConnectTo(matchFinder.matchmakerClient.ticketId, matchType);
						yield return matchFinder.ConnectToMatchWithTicketId(matchFinder.matchmakerClient.ticketId, matchFinder.GetPlayersForConnection(), matchType);
					}
					else if (matchFinder.matchmakerClient.lastError != MatchmakerClient.Error.Cancelled)
					{
						Debug.Log("OH IS thiS ITE");

						typeof(MatchFinder).GetMethod("OnJoinedServerFailure", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(matchFinder, new object[] { matchFinder.matchmakerClient.lastError });
					}
				}
				else if (matchFinder.matchmakerClient.lastError != 0)
				{
					Debug.Log("MAYBE A PROBLEM");

					typeof(MatchFinder).GetMethod("OnJoinedServerFailure", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(matchFinder, new object[] { matchFinder.matchmakerClient.lastError });
				}
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
			__result = myEnumerator.GetEnumerator();
		}

	}

	[HarmonyPatch(typeof(MatchFinder), "StartMatchmaking")]
	public class Patches2
	{
		static void Postfix(MatchFinder __instance, MatchType matchType)
		{
			GameManager.GMInstance.StartCoroutine(new Patches.SimpleEnumerator(__instance.GetPlayersForMatch(), matchType, GameManager.GMInstance.partyManager.GetParty().teamId, __instance.matchmakerClient).doRealMM(__instance));
		}
	}

	[HarmonyPatch(typeof(PartyManager))]
	[HarmonyPatch("NotifyPartyStartedMatchmaking")]
	class Testing_Patch
	{
		static void Prefix(int estimatedWaitTime)
		{
			Debug.Log("RUNNNING THE UPDATE!!!!!!!!!!!!!!!!!!!!!!!!!! " + estimatedWaitTime);
		}
	}

}