using GameLogic;
using GameSparks;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using GameSparks.Core;
using GameSparks.Platforms;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WebSocket4Net;
using WebSocketSharp;

namespace WebSocket
{

    public class envconfmod
    {
        [HarmonyPatch(typeof(EnvironmentConfig))]
        [HarmonyPatch("GetGameSparksSettingsObject")]
        public static class SetGSSettings_Patch
        {
            public static bool Prefix(Env env, GameSparksSettings __result)
            {
                Debug.Log("==========GETTING GS SETTINGS===============");

                GameSparksSettings gameSparksSettings = Resources.Load<GameSparksSettings>($"GameSparks/{env}");
                if (gameSparksSettings == null)
                {
                    Debug.LogError("Could not load gamesparks env: " + env);
                }
                Uri test = new Uri(GameManager.GMInstance.cvars.webServiceUrl);
                Debug.Log("Domain part : " + test.Host);
                string url = "ws://" + test.Host + ":3000";
                typeof(GameSparksSettings)
                    .GetField("previewServiceUrlBase", BindingFlags.Static | BindingFlags.NonPublic)
                    .SetValue(null, url);
                Debug.Log(GameSparksSettings.ServiceUrl);
                __result = gameSparksSettings;
                return false;
            }

        }
    }

    public class gsinstancemod
    {
        [HarmonyPatch(typeof(GSInstance))]
        [HarmonyPatch("BuildServiceUrl")]
        public static class SetGSSettings_Patch
        {
            public static bool Prefix(ref IGSPlatform platform, ref string __result)
            {
                Uri test = new Uri(GameManager.GMInstance.cvars.webServiceUrl);
                string url = "ws://" + test.Host + ":3000";

                __result = url;
                return false;
            }

        }

        [HarmonyPatch(typeof(GSInstance))]
        [HarmonyPatch("Send")]
        public static class GSSender_Patch
        {
            public static bool Prefix(GSInstance __instance, bool ____ready, string ____currentSocketUrl, GSRequest request)
            {
                Debug.Log("==========WE DID TRY TO SEND A MESSAGE THO===============");
                Debug.Log(request.JSON);
                
                return true;
            }
        }

    }
}