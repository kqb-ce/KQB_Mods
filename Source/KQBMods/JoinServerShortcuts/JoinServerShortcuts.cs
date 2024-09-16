using HarmonyLib;
using Newtonsoft.Json;
using System;
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
            try
            {
                if (m_CommunityServerDict == null)
                {
                    using (WebClient wc = new WebClient())
                    {
                        string json = wc.DownloadString("https://kqbfileserver.fly.dev/servers.json");
                        m_CommunityServerDict = JsonConvert.DeserializeObject<Dictionary<string, CommunityServerConnection>>(json);
                    }
                }
                if (p[0].ToLower().Equals("list"))
                {
                    Debug.Log((object)"Available Servers: \n");
                    foreach (KeyValuePair<string, CommunityServerConnection> item in m_CommunityServerDict)
                    {
                        Debug.Log((object)(item.Key + " - JoinServer " + item.Value.ip + " " + item.Value.port));
                    }
                } 
                else
                {
                    if (UIManager.Instance == null)
                    {
                        Debug.Log("cannot connect to server from this screen, please return to main menu.");
                        return false;
                    }
                    else 
                    {
                        CommunityServerConnection conn = new CommunityServerConnection();
                        m_CommunityServerDict.TryGetValue(p[0], out conn);
                        UIManager.Instance.DirectConnectToServer(conn.ip, conn.port, loopback: false);
                    }

                }

                return false;
            }
            catch (Exception e) {
                Debug.Log("Error in JoinServerShortcuts mod: ");
                Debug.Log(e.Message);
                return true;
            }
        }
    }

    public class CommunityServerConnection
    {
        public ushort port;
        public string ip;
    }

}
