using HarmonyLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace JoinServerShortcuts
{
    [HarmonyPatch(typeof(DevConCommands))]
    [HarmonyPatch("JoinServer")]
    public static class JoinServer_Patch
    {
        public static Dictionary<string, CommunityServerConnection> m_CommunityServerDict;

        public static bool Prefix(string[] p)
        {
            if (p.Length >= 2)
            {   
              return true;
            }

            if( m_CommunityServerDict == null)
            {
                using (WebClient wc = new WebClient())
                {
                    string json = wc.DownloadString("https://kqbfileserver.fly.dev/servers.json");
                    m_CommunityServerDict = JsonConvert.DeserializeObject<Dictionary<string, CommunityServerConnection>>(json);
                }
            }
            CommunityServerConnection conn = new CommunityServerConnection();
            m_CommunityServerDict.TryGetValue(p[0], out conn);
            UIManager.Instance.DirectConnectToServer(conn.ip, conn.port, loopback: false);
                  
          return false;
        }
    }

    public class CommunityServerConnection
    {
        public ushort port;
        public string ip;
    }

}
