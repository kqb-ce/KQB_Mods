using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamClientMod
{
	[HarmonyPatch(typeof(LB_AchievementManager))]
	[HarmonyPatch("PullAchievementsGameSparks")]
	public static class PullAchievementsGameSparks_Patch
	{
		public static bool Prefix(bool ___isFinishedGsPull, Action onSuccess = null, Action onFailure = null)
		{
			// TODO decide if achievements get implemented eventually
			LB_AchievementManager achievementManager = GameManager.GMInstance.achievementManager;
			___isFinishedGsPull = true;
			onSuccess?.Invoke();
			return false;
		}
	}
}
