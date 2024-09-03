using GameSparks.Core;
using Nakama;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SteamClientMod
{
    class NakamaUtils
    {
        public static string serverkey = "abetterserverkey";
        private static string nakamaUrl = "kqb-nakama.fly.dev";
        public GameLogic.Profile ConvertUserToProfile(ISession session, string steamId)
        {
            GSRequestData data = new GSRequestData();
            foreach( var kv in session.Vars)
            {
                data.AddString(kv.Key, kv.Value);
            }
            GSRequestData resData = new GSRequestData();
            resData.Add("profile", data);
            Debug.Log("Profile: \n" + resData.GetGSData("profile").JSON);
            GameLogic.Profile profile = JsonConvert.DeserializeObject<GameLogic.Profile>(resData.GetGSData("profile").JSON);
            profile.playerId = session.UserId;
            profile.externalIds = new GameLogic.Profile.ExternalIds();
            profile.externalIds.steam = steamId;
            return profile;
        }

        public static Client GetClient()
        {
            return new Client("https", nakamaUrl, 7350, serverkey, UnityWebRequestAdapter.Instance);
        }

        public static ISession RestoreSession()
        {
            return Session.Restore(PlayerPrefs.GetString("nakama.authToken", null), PlayerPrefs.GetString("nakama.refreshToken", null));
        }

        public async Task<IApiStorageObjects> GetFromStorage(string collection, string key)
        {
            ISession session = RestoreSession();
            return await GetClient().ReadStorageObjectsAsync(session,
                new[] {
                  new StorageObjectId {
                   Collection = collection,
                   Key = key,
                   UserId = session.UserId
                  }
                });
        }

        public GameLogic.Profile AddPreferencesToProfile(GameLogic.Profile profile, IApiStorageObjects objects)
        {
            //TODO get party prefs for each user
            foreach (IApiStorageObject so in objects.Objects)
            {
                Dictionary<string, string> prefs = Nakama.TinyJson.JsonParser.FromJson<Dictionary<string, string>>(so.Value);
                profile.allowFriendsToJoinParty = Convert.ToBoolean(prefs["allowFriendsToJoinParty"]);
                profile.status = (GameLogic.Profile.Status)Enum.Parse(typeof(GameLogic.Profile.Status), prefs["status"]);
                profile.allowSpectateCustomMatch = Convert.ToBoolean(prefs["allowSpectateCustomMatch"]);
                profile.currentNetworkingPreferences = int.Parse(prefs["currentNetworkingPreferences"]);
                profile.allowFriendsToJoinCustomMatch = Convert.ToBoolean(prefs["allowFriendsToJoinCustomMatch"]);
                profile.allowFriendsOfFriendsToJoinParty = Convert.ToBoolean(prefs["allowFriendsOfFriendsToJoinParty"]);
            }
            return profile;
        }
    }
}