using HarmonyLib;
using LiquidBit.KillerQueenX;
using Steamworks;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace ServerOnly
{

    [HarmonyPatch(typeof(SteamLobbyManager))]
    [HarmonyPatch("OnLobbyEnter_Internal")]
    public static class GMP2P_Patch
    {
        public static void Postfix(LobbyEnter_t callback)
        {
            UnityEngine.Debug.Log("-----------DOING THIS----------");

            string[] commandLineArgs = Environment.GetCommandLineArgs();
            for (int i = 1; i < commandLineArgs.Length; i++)
            {
                UnityEngine.Debug.Log(commandLineArgs[i]);
                if (commandLineArgs[i] == "--pass")
                {
                    UnityEngine.Debug.Log("setting password");
                    SteamLobbyManager.Instance.SetPrivacy(true, LobbyKey.Hash(commandLineArgs[i + 1]), true);
                    break;
                }
            }
        }

    }

    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("Start")]
    public static class GMStart_Patch
    {
        public static void Prefix(GameManager __instance)
        {
            QualitySettings.vSyncCount = 0;
            GMData addData = __instance.GetTime();
            addData.time = 0f;
            Application.targetFrameRate = Convert.ToInt32(1f / Time.fixedDeltaTime);
            AudioListener.pause = true;
            AudioListener.volume = 0;
        }

    }

    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("Update")]
    public static class GMUpdate_Patch
    {
        public static void Postfix(GameManager __instance, ref GameLogic.GameServer ___localGameServer)
        {
            GMData addData = __instance.GetTime();
            addData.time += Time.deltaTime;
            while (addData.time >= 3f)
            {
                if ((___localGameServer == null || !___localGameServer.isRunning) && UIManager.Instance != null)
                {
                    UIManager.Instance.StartCustomMatchFromServerBrowser();
                }
                addData.time -= 3f;
            }

        }

    }

    [HarmonyPatch(typeof(RemoteNetworkManager))]
    [HarmonyPatch("ConnectToServer")]
    public static class ConnectToServer_Patch
    {
        public static bool Prefix(GameLogic.WebService.Model.Connection connection, ref bool spectator)
        {
            spectator = true;
            return true;
        }

    }

    [HarmonyPatch(typeof(LoadingBarScript))]
    [HarmonyPatch("Update")]
    public static class LoadingBarScript_Patch
    {
        public static void Postfix(LoadingBarScript __instance)
        {
            bool toGame = Traverse.Create(__instance).Field("toGame").GetValue<bool>();
            bool loaded = Traverse.Create(__instance).Field("loaded").GetValue<bool>();
            bool _canContinue = Traverse.Create(__instance).Field("_canContinue").GetValue<bool>();

            if (!toGame && loaded && _canContinue)
            {
                toGame = true;
                __instance.StartCoroutine(GoToMainMenu());
                Traverse.Create(__instance).Field("allStepsCompleted").SetValue(true);
            }

        }

        private static IEnumerator GoToMainMenu()
        {
            yield return new WaitForSecondsRealtime(0.8f);
            GameManager.GMInstance.platformClient.CheckPendingActivationAction(PendingActivationType.GameLaunch);
            GameManager.GMInstance.stringSanitizer.Reload();
        }

    }

    [HarmonyPatch(typeof(SteamFriends), "GetPersonaName")]
    public static class GetPersonaName_Patch
    {
        static bool Prefix(ref string __result, object[] __args)
        {
            if (IsMethodInCurrentStack("OnLobbyCreated_Internal"))
            {

                string[] commandLineArgs = Environment.GetCommandLineArgs();
                for (int i = 1; i < commandLineArgs.Length; i++)
                {
                    if (commandLineArgs[i] == "--lobby")
                    {
                        __result = commandLineArgs[++i];
                        break;
                    }
                }

                return false;
            }

            return true;
        }
        private static bool IsMethodInCurrentStack(string methodName)
        {
            // Capture the current call stack
            StackTrace stackTrace = new StackTrace();

            // Inspect the frames for a matching method name
            return stackTrace.GetFrames()
                ?.Any(frame => frame.GetMethod()?.Name == methodName) ?? false;
        }
    }

    public class GMData
    {
        public float time;
    }

    public static class GMExtensions
    {
        private static readonly ConditionalWeakTable<GameManager, GMData> Table =
            new ConditionalWeakTable<GameManager, GMData>();

        public static GMData GetTime(this GameManager instance)
        {
            return Table.GetOrCreateValue(instance);
        }
    }
}
