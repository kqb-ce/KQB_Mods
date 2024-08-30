using HarmonyLib;
using LiquidBit.KillerQueenX;
using System.Reflection;
using UnityEngine;

namespace SteamlessClientMod
{
    [HarmonyPatch(typeof(DevConCommands))]
    [HarmonyPatch("JoinServer")]
    public static class JoinServer_Patch
    {
        public static bool Prefix(DevConCommands __instance, string[] p)
        {
          if (p.Length >= 2)
		  {   
            return true;
          }
          using (WebClient wc = new WebClient())
          {
            string json = wc.DownloadString("https://kqbfileserver.fly.dev/servers.json");
            Dictionary<string, CommunityServerConnection> json_Dictionary = (new JavaScriptSerializer()).Deserialize<Dictionary<string, CommunityServerConnection>>(json);

            CommunityServerConnection conn = json_Dictionary.getField(p[0])
            UIManager.Instance.DirectConnectToServer(conn.ip, conn.port, loopback: false);
          }        
          return false;
        }
    }

    public class CommunityServerConnection
    {
        public ushort port;
        public string ip;
    }

}
