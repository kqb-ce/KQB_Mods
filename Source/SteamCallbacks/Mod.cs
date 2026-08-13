using HarmonyLib;
using LiquidBit.KillerQueenX;
using Mono.Cecil;
using NetLib;
using Steamworks;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

namespace SteamCallbacks
{
    [HarmonyPatch(typeof(SteamP2PHostRelay))]
    [HarmonyPatch("Update")]
    public static class P2PUpdate_Patch
    {

        private static readonly MethodInfo CloseRelayedConnectionRef =
        AccessTools.Method(typeof(SteamP2PHostRelay), "CloseRelayedConnection");

        private static readonly MethodInfo ForwardToSteamRef =
AccessTools.Method(typeof(SteamP2PHostRelay), "ForwardToSteam");
        public static bool Prefix(SteamP2PHostRelay __instance, ref bool ___isListening, ref List<HSteamNetConnection> ___pendingRemoves, ref Dictionary<HSteamNetConnection, object> ___connections, ref float ___pingPollTimer)
        {
            if (!___isListening)
            {
                return false;
            }

            float unscaledDeltaTime = Time.unscaledDeltaTime;

            // 1. Optimize connection removal (Avoid double lookup)
            if (___pendingRemoves.Count > 0)
            {
                for (int i = 0; i < ___pendingRemoves.Count; i++)
                {
                    HSteamNetConnection key = ___pendingRemoves[i];
                    if (___connections.TryGetValue(key, out object relayed))
                    {
                        CloseRelayedConnectionRef.Invoke(__instance, [relayed]);
                        ___connections.Remove(key);
                    }
                }
                ___pendingRemoves.Clear();
            }

            if (___connections.Count > 0)
            {
                // 2. Iterate connections using direct KeyValuePair struct enumeration
                ___pingPollTimer += unscaledDeltaTime;

                foreach (var kvp in ___connections)
                {
                    var traverse = Traverse.Create(kvp.Value);

                    if (___pingPollTimer >= 2f)
                    {
                        ___pingPollTimer = 0f;

                        // Cache struct defaults outside loop
                        SteamNetConnectionRealTimeStatus_t status = default;
                        SteamNetConnectionRealTimeLaneStatus_t laneStatus = default;

                        if (SteamNetworkingSockets.GetConnectionRealTimeStatus(traverse.Field("steamConn").GetValue<HSteamNetConnection>(), ref status, 0, ref laneStatus) == EResult.k_EResultOK
                            && status.m_nPing >= 0)
                        {
                            ConcurrentDictionary<ulong, int> dict = Traverse.Create(typeof(SteamP2PHostRelay)).Field("pingBySteamId").GetValue<ConcurrentDictionary<ulong, int>>();
                            dict[traverse.Field("remoteSteamId").GetValue<CSteamID>().m_SteamID] = status.m_nPing;
                        }

                    }
                    //object value = kvp.Value;
                    traverse.Field("time").SetValue(traverse.Field("time").GetValue<double>() + (double)unscaledDeltaTime);
                    NetLib.Client netLibClient = traverse.Field("netLibClient").GetValue<NetLib.Client>();
                    netLibClient.Service(traverse.Field("time").GetValue<double>());

                    while (netLibClient.GetEvent(out NetLib.NetworkEvent networkEvent))
                    {
                        switch (networkEvent.type)
                        {
                            case NetLib.NetworkEvent.Type.Connect:
                                traverse.Field("netLibConnected").SetValue(true);
                                // String interpolation or conditional log prevents string.Format heap allocations
                                UnityEngine.Debug.Log($"[SteamP2PRelay] Local NetLib connection established for {traverse.Field("remoteSteamId").GetValue<CSteamID>()}");
                                break;

                            case NetLib.NetworkEvent.Type.Receive:
                                Traverse.Create(__instance)
                     .Method("ForwardToSteam", traverse.Field("steamConn").GetValue<HSteamNetConnection>(), networkEvent.message)
                     .GetValue();
                                break;

                            case NetLib.NetworkEvent.Type.Disconnect:
                                traverse.Field("netLibConnected").SetValue(false);
                                UnityEngine.Debug.Log($"[SteamP2PRelay] Local NetLib connection lost for  {traverse.Field("remoteSteamId").GetValue<CSteamID>()}");
                                SteamNetworkingSockets.CloseConnection(traverse.Field("steamConn").GetValue<HSteamNetConnection>(), 0, "Server disconnected", true);
                                ___pendingRemoves.Add(kvp.Key);
                                break;
                        }

                        netLibClient.FreeEvent(networkEvent);
                    }
                    Traverse.Create(__instance)
                     .Method("ReceiveFromSteamAndForward", kvp.Value)
                     .GetValue();
                }

            }
            return false;
        }

        [HarmonyPatch(typeof(SteamP2PHostRelay))]
        [HarmonyPatch("ForwardToSteam")]
        public static class ForwardToSteam_Patch
        {
            public static bool Prefix(HSteamNetConnection conn, NetLib.Message message)
            {

                //int payloadSize = message.bitLength / 8;
                //int totalSize = 1 + payloadSize;
                //int sendFlags = (message.channel == 0) ? 5 : 8; // 5 = Reliable, 8 = Unreliable/NoNagle depending on flags

                //// Threshold for safe stack allocation (1KB is safe and covers 99%+ of game network messages)
                //if (totalSize <= 1024)
                //{
                //    // Zero GC allocation stack buffer
                //    Span<byte> stackBuffer = stackalloc byte[totalSize];
                //    stackBuffer[0] = (byte)message.channel;

                //    // Fast memory copy
                //    message.data.AsSpan(0, payloadSize).CopyTo(stackBuffer.Slice(1));

                //    unsafe
                //    {
                //        fixed (byte* ptr = stackBuffer)
                //        {
                //            SteamNetworkingSockets.SendMessageToConnection(
                //                conn,
                //                (IntPtr)ptr,
                //                (uint)totalSize,
                //                sendFlags,
                //                out long _
                //            );
                //        }
                //    }
                //}
                //else
                //{
                //    // Fallback for large packets: Rent from shared array pool (Zero GC garbage)
                //    byte[] rentedArray = ArrayPool<byte>.Shared.Rent(totalSize);
                //    try
                //    {
                //        rentedArray[0] = (byte)message.channel;
                //        Buffer.BlockCopy(message.data, 0, rentedArray, 1, payloadSize);

                //        unsafe
                //        {
                //            fixed (byte* ptr = rentedArray)
                //            {
                //                SteamNetworkingSockets.SendMessageToConnection(
                //                    conn,
                //                    (IntPtr)ptr,
                //                    (uint)totalSize,
                //                    sendFlags,
                //                    out long _
                //                );
                //            }
                //        }
                //    }
                //    finally
                //    {
                //        ArrayPool<byte>.Shared.Return(rentedArray);
                //    }
                //}



                return false;
            }
        }

        [HarmonyPatch(typeof(SteamP2PHostRelay))]
        [HarmonyPatch("ReceiveFromSteamAndForward")]
        public static class ReceiveFromSteamAndForward_Patch
        {
            public static bool Prefix(object relayed)
            {
                return false;
                if (!relayed.netLibConnected)
                {
                    return;
                }

                int num = SteamNetworkingSockets.ReceiveMessagesOnConnection(
                    relayed.steamConn,
                    this.messagePointers,
                    this.messagePointers.Length
                );

                for (int i = 0; i < num; i++)
                {
                    IntPtr msgPtr = this.messagePointers[i];

                    unsafe
                    {
                        // Cast direct pointer to struct without calling Marshal.PtrToStructure
                        SteamNetworkingMessage_t* msg = (SteamNetworkingMessage_t*)msgPtr;
                        int cbSize = msg->m_cbSize;

                        if (cbSize > 1)
                        {
                            // Read channel directly from payload pointer
                            byte channel = *(byte*)msg->m_pData;
                            int payloadSize = cbSize - 1;

                            // Option A: If netLibClient supports NativeArray/ReadOnlySpan/IntPtr (Zero Allocation)
                            // IntPtr payloadPtr = msg->m_pData + 1;
                            // relayed.netLibClient.SendMessage((int)channel, payloadPtr, payloadSize);

                            // Option B: Single byte[] allocation offset by 1 byte
                            byte[] payload = new byte[payloadSize];
                            Marshal.Copy(msg->m_pData + 1, payload, 0, payloadSize);

                            relayed.netLibClient.SendMessage((int)channel, payload, payloadSize * 8);
                        }

                        // Release pointer static method call directly from struct pointer
                        SteamNetworkingMessage_t.Release(msgPtr);
                    }
                }
            }
        }

            [HarmonyPatch(typeof(CallbackDispatcher))]
        [HarmonyPatch("RunFrame")]
        public static class Runframe_Patch
        {
            public static bool Prefix(bool isGameServer)
            {
                if (SceneManager.GetActiveScene().name == "TiledScene")
                {
                    return false;
                }
                return true;
            }
        }

        //[HarmonyPatch(typeof(CallbackDispatcher))]
        //[HarmonyPatch("RunFrame")]
        //public static class Runframe_Patch
        //{
        //    public static List<string> __preClientCbs = new List<string>();
        //    public static List<string> __preSrvCbs = new List<string>();
        //    public static bool Prefix(bool isGameServer, ref Dictionary<int, List<Callback>> ___m_registeredGameServerCallbacks, ref Dictionary<int, List<Callback>> ___m_registeredCallbacks, out Stopwatch __state)
        //    {
        //        //if(SteamLobbyManager.Instance.GetStatus()("in_game");)
        //        if (SceneManager.GetActiveScene().name == "TiledScene")
        //        {
        //            __state = new Stopwatch();
        //            return false;

        //        }
        //        foreach (var (num2, cbList) in ___m_registeredGameServerCallbacks)
        //        {
        //            foreach (Callback cb in cbList)
        //            {
        //                __preSrvCbs.Add(cb.ToString());
        //            }
        //        }

        //        foreach (var (num2, cbList) in ___m_registeredCallbacks)
        //        {
        //            foreach (Callback cb in cbList)
        //            {
        //                __preClientCbs.Add(cb.ToString());
        //            }
        //        }
        //        __state = new Stopwatch();
        //        __state.Start();
        //        return true;

        //    }

        //    static void Postfix(bool isGameServer, ref Dictionary<int, List<Callback>> ___m_registeredGameServerCallbacks, ref Dictionary<int, List<Callback>> ___m_registeredCallbacks, Stopwatch __state)
        //    {
        //        __state.Stop();

        //        if (__state.ElapsedMilliseconds > 4)
        //        {
        //            UnityEngine.Debug.Log($"Execution Time: {__state.ElapsedMilliseconds} ms");
        //            UnityEngine.Debug.Log($"===starting callbacks=======\nIsServer: {isGameServer}");
        //            UnityEngine.Debug.Log($"srvCalbacks length: pre: {__preSrvCbs.Count} post: {___m_registeredGameServerCallbacks.Count}");
        //            UnityEngine.Debug.Log($"clientCalbacks length: pre: {__preClientCbs.Count} post: {___m_registeredCallbacks.Count}");

        //            UnityEngine.Debug.Log("Pre server calls: ");
        //            foreach (string str in __preSrvCbs)
        //            {
        //                UnityEngine.Debug.Log(str);
        //            }

        //            UnityEngine.Debug.Log("Post server calls: ");
        //            foreach (var (num2, cbList) in ___m_registeredGameServerCallbacks)
        //            {
        //                foreach (Callback cb in cbList)
        //                {
        //                    UnityEngine.Debug.Log(cb.ToString());
        //                }
        //            }
        //            UnityEngine.Debug.Log("Pre client calls: ");
        //            foreach (string str in __preClientCbs)
        //            {
        //                UnityEngine.Debug.Log(str);
        //            }
        //            UnityEngine.Debug.Log("Post client calls: ");
        //            foreach (var (num2, cbList) in ___m_registeredCallbacks)
        //            {
        //                foreach (Callback cb in cbList)
        //                {
        //                    UnityEngine.Debug.Log(cb.ToString());
        //                }
        //            }
        //        }
        //        __preSrvCbs.Clear();
        //        __preClientCbs.Clear();
        //    }

        //}
    }
}