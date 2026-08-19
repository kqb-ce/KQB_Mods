using GameLogic;
using HarmonyLib;
using LiquidBit.KillerQueenX;
using NetLib;
using Steamworks;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SteamCallbacks
{
    // optimize SteamP2PHostRelay
    [HarmonyPatch(typeof(SteamP2PHostRelay))]
    [HarmonyPatch("Update")]
    public static class P2PUpdate_Patch
    {

        private static readonly MethodInfo CloseRelayedConnectionRef =
        AccessTools.Method(typeof(SteamP2PHostRelay), "CloseRelayedConnection");

        private static readonly MethodInfo ForwardToSteamRef =
AccessTools.Method(typeof(SteamP2PHostRelay), "ForwardToSteam");
        private static readonly MethodInfo ReceiveFromSteamAndForwardRef =
AccessTools.Method(typeof(SteamP2PHostRelay), "ReceiveFromSteamAndForward");
        private static readonly Type reyaledConnType = AccessTools.TypeByName("LiquidBit.KillerQueenX.SteamP2PHostRelay+RelayedConnection");

        private static readonly FieldInfo steamConnFieldInfo = reyaledConnType.GetField("steamConn", BindingFlags.Public | BindingFlags.Instance);

        private static readonly FieldInfo remoteSteamIdFieldInfo = reyaledConnType.GetField("remoteSteamId", BindingFlags.Public | BindingFlags.Instance);

        private static readonly FieldInfo timeFieldInfo = reyaledConnType.GetField("time", BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo netLibClientInfo = reyaledConnType.GetField("netLibClient", BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo netLibConnectedInfo = reyaledConnType.GetField("netLibConnected", BindingFlags.Public | BindingFlags.Instance);

        private static readonly AccessTools.FieldRef<object, HSteamNetConnection> GetSteamConn =
        AccessTools.FieldRefAccess<object, HSteamNetConnection>(steamConnFieldInfo);

        private static readonly AccessTools.FieldRef<object, CSteamID> GetRemoteSteamIdFieldInfo =
AccessTools.FieldRefAccess<object, CSteamID>(remoteSteamIdFieldInfo);

        private static readonly AccessTools.FieldRef<object, double> GetTime =
AccessTools.FieldRefAccess<object, double>(timeFieldInfo);
        private static readonly AccessTools.FieldRef<object, NetLib.Client> GetClient =
AccessTools.FieldRefAccess<object, NetLib.Client>(netLibClientInfo);

        private static readonly AccessTools.FieldRef<object, bool> GetNetLibConnected =
AccessTools.FieldRefAccess<object, bool>(netLibConnectedInfo);
        public static bool Prefix(SteamP2PHostRelay __instance, ref bool ___isListening, ref List<HSteamNetConnection> ___pendingRemoves, ref Dictionary<HSteamNetConnection, object> ___connections, ref float ___pingPollTimer, ref ConcurrentDictionary<ulong, int> ___pingBySteamId)
        {
            if (!___isListening)
            {
                return false;
            }

            float unscaledDeltaTime = Time.unscaledDeltaTime;

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
                ___pingPollTimer += unscaledDeltaTime;
                bool shouldPing = ___pingPollTimer >= 2f;
                if (shouldPing)
                {
                    ___pingPollTimer = 0f; // Reset outside the loop so ALL connections get polled
                }
                SteamNetConnectionRealTimeStatus_t status = default;
                SteamNetConnectionRealTimeLaneStatus_t laneStatus = default;

                foreach (var kvp in ___connections)
                {
                    HSteamNetConnection steamConn = GetSteamConn(kvp.Value);
                    CSteamID remoteSteamId = GetRemoteSteamIdFieldInfo(kvp.Value);

                    GetTime(kvp.Value) = GetTime(kvp.Value) + (double)unscaledDeltaTime;
                    NetLib.Client netLibClient = GetClient(kvp.Value);
                    netLibClient.Service(GetTime(kvp.Value));

                    while (netLibClient.GetEvent(out NetLib.NetworkEvent networkEvent))
                    {
                        switch (networkEvent.type)
                        {
                            case NetLib.NetworkEvent.Type.Receive:
                                ForwardToSteamRef.Invoke(__instance, [steamConn, networkEvent.message]);
                                break;
                            case NetLib.NetworkEvent.Type.Connect:
                                GetNetLibConnected(kvp.Value) = true;
                                break;

                            case NetLib.NetworkEvent.Type.Disconnect:
                                GetNetLibConnected(kvp.Value) = false;
                                SteamNetworkingSockets.CloseConnection(steamConn, 0, "Server disconnected", true);
                                ___pendingRemoves.Add(kvp.Key);
                                break;
                        }

                        netLibClient.FreeEvent(networkEvent);
                    }
                    ReceiveFromSteamAndForwardRef.Invoke(__instance, [kvp.Value]);
                    if (shouldPing)
                    {

                        if (SteamNetworkingSockets.GetConnectionRealTimeStatus(steamConn, ref status, 0, ref laneStatus) == EResult.k_EResultOK
                            && status.m_nPing >= 0)
                        {
                            ___pingBySteamId[remoteSteamId.m_SteamID] = status.m_nPing;
                        }

                    }
                }

            }
            return false;
        }

        // optimize ForwardToSteam
        [HarmonyPatch(typeof(SteamP2PHostRelay))]
        [HarmonyPatch("ForwardToSteam")]
        public static class ForwardToSteam_Patch
        {
            public static bool Prefix(HSteamNetConnection conn, NetLib.Message message)
            {

                int payloadSize = message.bitLength / 8;
                int totalSize = 1 + payloadSize;
                int sendFlags = (message.channel == 0) ? 5 : 8;

                if (totalSize <= 1024)
                {
                    Span<byte> stackBuffer = stackalloc byte[totalSize];
                    stackBuffer[0] = (byte)message.channel;

                    message.data.AsSpan(0, payloadSize).CopyTo(stackBuffer.Slice(1));

                    unsafe
                    {
                        fixed (byte* ptr = stackBuffer)
                        {
                            SteamNetworkingSockets.SendMessageToConnection(
                                conn,
                                (IntPtr)ptr,
                                (uint)totalSize,
                                sendFlags,
                                out long _
                            );
                        }
                    }
                }
                else
                {
                    byte[] rentedArray = ArrayPool<byte>.Shared.Rent(totalSize);
                    try
                    {
                        rentedArray[0] = (byte)message.channel;
                        Buffer.BlockCopy(message.data, 0, rentedArray, 1, payloadSize);

                        unsafe
                        {
                            fixed (byte* ptr = rentedArray)
                            {
                                SteamNetworkingSockets.SendMessageToConnection(
                                    conn,
                                    (IntPtr)ptr,
                                    (uint)totalSize,
                                    sendFlags,
                                    out long _
                                );
                            }
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(rentedArray);
                    }
                }
                return false;
            }
        }

        //Optimize REceiveFromSteamAndForward
        [HarmonyPatch(typeof(SteamP2PHostRelay))]
        [HarmonyPatch("ReceiveFromSteamAndForward")]
        public static class ReceiveFromSteamAndForward_Patch
        {
            public static IntPtr[] array = new IntPtr[64];

            public static bool Prefix(SteamP2PHostRelay __instance, object relayed)
            {
                if (!GetNetLibConnected(relayed))
                {
                    return false;
                }

                int num = SteamNetworkingSockets.ReceiveMessagesOnConnection(
                    GetSteamConn(relayed),
                    array,
                    array.Length
                );

                for (int i = 0; i < num; i++)
                {
                    IntPtr msgPtr = array[i];

                    unsafe
                    {
                        SteamNetworkingMessage_t* msg = (SteamNetworkingMessage_t*)msgPtr;
                        int cbSize = msg->m_cbSize;

                        if (cbSize > 1)
                        {
                            byte channel = *(byte*)msg->m_pData;
                            int payloadSize = cbSize - 1;

                            byte[] payload = new byte[payloadSize];
                            Marshal.Copy(msg->m_pData + 1, payload, 0, payloadSize);
                            GetClient(relayed).SendMessage((int)channel, payload, payloadSize * 8);
                        }

                        SteamNetworkingMessage_t.Release(msgPtr);
                    }
                }
                return false;
            }
        }

        // Optimize SteamP2PClient
        [HarmonyPatch(typeof(SteamP2PClient))]
        [HarmonyPatch("Service")]
        public static class Service_Patch
        {
            public static bool Prefix(SteamP2PClient __instance, ref System.Collections.Generic.Queue<NetworkEvent> ___pendingEvents, ref HSteamNetConnection ___connection, ref IntPtr[] ___msgPtrs, double time)
            {
                if (!__instance.isConnected)
                {
                    return false;
                }

                int count = SteamNetworkingSockets.ReceiveMessagesOnConnection(___connection, ___msgPtrs, ___msgPtrs.Length);
                unsafe
                {
                    for (int i = 0; i < count; i++)
                    {
                        IntPtr msgPtr = ___msgPtrs[i];

                        SteamNetworkingMessage_t* netMsg = (SteamNetworkingMessage_t*)msgPtr;
                        int payloadSize = netMsg->m_cbSize;

                        if (payloadSize > 0)
                        {
                            byte* dataPtr = (byte*)netMsg->m_pData;
                            int channel = dataPtr[0];
                            int dataSize = payloadSize - 1;

                            byte[] messageData = new byte[dataSize];

                            Marshal.Copy((IntPtr)(dataPtr + 1), messageData, 0, dataSize);

                            ___pendingEvents.Enqueue(new NetworkEvent
                            {
                                type = NetworkEvent.Type.Receive,
                                clientIndex = 0,
                                message = new Message
                                {
                                    data = messageData,
                                    bitLength = dataSize * 8,
                                    channel = channel
                                }
                            });
                        }

                        SteamNetworkingMessage_t.Release(msgPtr);
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(SteamP2PClient))]
        [HarmonyPatch("SendMessage")]
        public static class SendMessage_Patch
        {
            public static bool Prefix(SteamP2PClient __instance, HSteamNetConnection ___connection, int channel, byte[] data, int bitLength)
            {
                if (!__instance.isConnected)
                {
                    return false;
                }

                int byteLength = bitLength / 8;
                int totalSize = 1 + byteLength;
                int sendFlags = (channel == 0) ? 5 : 8;

                if (totalSize <= 1024)
                {
                    Span<byte> stackBuffer = stackalloc byte[totalSize];
                    stackBuffer[0] = (byte)channel;

                    data.AsSpan(0, byteLength).CopyTo(stackBuffer.Slice(1));

                    unsafe
                    {
                        fixed (byte* ptr = stackBuffer)
                        {
                            SteamNetworkingSockets.SendMessageToConnection(
                                ___connection,
                                (IntPtr)ptr,
                                (uint)totalSize,
                                sendFlags,
                                out long _
                            );
                        }
                    }
                }
                else
                {
                    byte[] rentedArray = ArrayPool<byte>.Shared.Rent(totalSize);
                    try
                    {
                        rentedArray[0] = (byte)channel;
                        Buffer.BlockCopy(data, 0, rentedArray, 1, byteLength);

                        unsafe
                        {
                            fixed (byte* ptr = rentedArray)
                            {
                                SteamNetworkingSockets.SendMessageToConnection(
                                    ___connection,
                                    (IntPtr)ptr,
                                    (uint)totalSize,
                                    sendFlags,
                                    out long _
                                );
                            }
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(rentedArray);
                    }
                }
                return false;
            }
        }

        // optimize frame end
        [HarmonyPatch(typeof(MatchScoreUI), "CaptureImageIEnumerator")]
        public static class CaptureImageIEnumerator_Patch
        {
            private static readonly WaitForEndOfFrame CachedWaitForEndOfFrame = new WaitForEndOfFrame();
            public static bool Prefix(MatchScoreUI __instance, ref IEnumerator __result, ref int ___totalPoints)
            {
                __result = CustomCaptureLogic(__instance, ___totalPoints);

                return false;
            }

            private static IEnumerator CustomCaptureLogic(MatchScoreUI instance, int totalPoints)
            {
                yield return CachedWaitForEndOfFrame;

 
                if (Camera.main == null) yield break;
 
                int width = Camera.main.pixelWidth;
                int height = Camera.main.pixelHeight;

                if (instance.screenshotForeground.sprite != null)
                {
                    MatchScoreUI.Destroy(instance.screenshotForeground.sprite.texture);
                    MatchScoreUI.Destroy(instance.screenshotForeground.sprite);
                }

                Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);

                texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture2D.Apply(false, true);

                Sprite sprite = Sprite.Create(
                    texture2D,
                    new Rect(0f, 0f, width, height),
                    new Vector2(0.5f, 0.5f)
                );

                instance.screenshotForeground.sprite = sprite;
                GameManager.GMInstance.AddPostMatchCaptureToNextLevel(sprite, totalPoints - 1);
            }
        }

        // optimize foliageextractor
        [HarmonyPatch(typeof(FoliageExtractor), "Start")]
        public static class FoliageExtractorPatch
        {
            private static readonly MethodInfo GetBoundingRectFromPointsRef =
AccessTools.Method(typeof(FoliageExtractor), "GetBoundingRectFromPoints");


            private static GameObject makeGO(FoliageExtractor instance, GameObject baseGameObject, ref Rect boundingRect, int[] triangles, Vector3[] vertices, Vector2[] uvs, Vector2[] uv2, Vector2[] uv3, Color[] colors, int[] trianglesInCollider, int trianglesInColliderCount)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(baseGameObject, instance.transform);
                MeshFilter component = gameObject.GetComponent<MeshFilter>();
                FoliageWaverPlayerMovement component2 = gameObject.GetComponent<FoliageWaverPlayerMovement>();
                Rigidbody2D rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
                component2.windMultiplier = instance.windMultiplier;
                rigidbody2D.bodyType = RigidbodyType2D.Static;
                BoxCollider2D component3 = gameObject.GetComponent<BoxCollider2D>();
                component3.offset = boundingRect.center;
                component3.size = boundingRect.size;
                Mesh mesh = new Mesh();
                int[] array = new int[trianglesInColliderCount * 3];
                int num = 0;
                for (int i = 0; i < trianglesInColliderCount; i++)
                {
                    int num2 = trianglesInCollider[i] * 3;
                    array[num++] = triangles[num2];
                    array[num++] = triangles[num2 + 1];
                    array[num++] = triangles[num2 + 2];
                }
                mesh.vertices = vertices;
                mesh.uv = uvs;
                mesh.uv2 = uv2;
                mesh.uv3 = uv3;
                mesh.colors = colors;
                mesh.triangles = array;
                component.mesh = mesh;
                return gameObject;
            }
        
            public static bool Prefix(FoliageExtractor __instance)
            {
                if (GameManager.GMInstance.lowPerformanceMode)
                    return false;
                ref GameObject baseGameObject = ref AccessTools.FieldRefAccess<FoliageExtractor, GameObject>("baseGameObject")(__instance);

                baseGameObject = GameManager.GMInstance.assetSystem.LoadAsset<GameObject>("Foliage");

                PolygonCollider2D[] colliders = __instance.gameObject.GetComponentsInChildren<PolygonCollider2D>();
                if (colliders.Length == 0)
                    return false;

                MeshFilter meshFilter = __instance.gameObject.GetComponentInChildren<MeshFilter>();
                if (meshFilter == null)
                    return false;

                Renderer sourceRenderer = meshFilter.GetComponent<Renderer>();
                Mesh sourceMesh = meshFilter.mesh;

                Vector3[] vertices = sourceMesh.vertices;
                Vector2[] uv = sourceMesh.uv;
                int[] triangles = sourceMesh.triangles;

                int vertCount = vertices.Length;
                int totalTriangles = triangles.Length / 3;

                Vector2[] uv2 = new Vector2[vertCount];
                Vector2[] uv3 = new Vector2[vertCount];
                Color[] colors = new Color[vertCount];

                System.Array.Fill(colors, Color.white);

                int[] matchedTriangleIndices = new int[totalTriangles];
                bool[] usedTriangles = new bool[totalTriangles];
                int totalUsedTrianglesCount = 0;

                Material sharedMat = sourceRenderer.sharedMaterial;
                int sortingLayerID = sourceRenderer.sortingLayerID;
                int sortingOrder = sourceRenderer.sortingOrder;

                foreach (PolygonCollider2D collider in colliders)
                {
                    Vector2[] colliderPoints = collider.points;
                    Rect bounds = (UnityEngine.Rect)GetBoundingRectFromPointsRef.Invoke(__instance, [colliderPoints, 0.01f]);

                    int matchedCount = 0;

                    for (int k = 0; k < totalTriangles; k++)
                    {
                        int triIdx = k * 3;
                        Vector3 p1 = vertices[triangles[triIdx]];
                        Vector3 p2 = vertices[triangles[triIdx + 1]];
                        Vector3 p3 = vertices[triangles[triIdx + 2]];

                        if (bounds.Contains(p1) && bounds.Contains(p2) && bounds.Contains(p3))
                        {
                            matchedTriangleIndices[matchedCount++] = k;

                            if (!usedTriangles[k])
                            {
                                usedTriangles[k] = true;
                                totalUsedTrianglesCount++;
                            }
                        }
                    }

                    if (matchedCount == 0)
                        continue;

                    float maxY = bounds.max.y;
                    float minY = bounds.min.y;
                    float centerX = bounds.center.x;

                    Vector2 uv2Val = new Vector2(maxY, minY);
                    Vector2 uv3Val = new Vector2(centerX, maxY);

                    for (int l = 0; l < matchedCount; l++)
                    {
                        int triIdx = matchedTriangleIndices[l] * 3;
                        int v1 = triangles[triIdx];
                        int v2 = triangles[triIdx + 1];
                        int v3 = triangles[triIdx + 2];

                        uv2[v1] = uv2Val;
                        uv2[v2] = uv2Val;
                        uv2[v3] = uv2Val;

                        uv3[v1] = uv3Val;
                        uv3[v2] = uv3Val;
                        uv3[v3] = uv3Val;
                    }

                    GameObject foliageGO = makeGO(__instance, baseGameObject, ref bounds, triangles, vertices, uv, uv2, uv3, colors, matchedTriangleIndices, matchedCount);
                    if (foliageGO != null && foliageGO.TryGetComponent<Renderer>(out Renderer foliageRenderer))
                    {
                        foliageRenderer.sharedMaterial = sharedMat;
                        foliageRenderer.sortingLayerID = sortingLayerID;
                        foliageRenderer.sortingOrder = sortingOrder;
                    }
                }

                int remainingTrianglesCount = totalTriangles - totalUsedTrianglesCount;
                int[] remainingTriangles = new int[remainingTrianglesCount * 3];
                int writeIdx = 0;

                for (int m = 0; m < totalTriangles; m++)
                {
                    if (!usedTriangles[m])
                    {
                        int triIdx = m * 3;
                        remainingTriangles[writeIdx++] = triangles[triIdx];
                        remainingTriangles[writeIdx++] = triangles[triIdx + 1];
                        remainingTriangles[writeIdx++] = triangles[triIdx + 2];
                    }
                }

                Mesh newMesh = new Mesh();
                newMesh.vertices = vertices;
                newMesh.uv = uv;
                newMesh.colors = colors;
                newMesh.triangles = remainingTriangles;

                meshFilter.mesh = newMesh;

                for (int j = 0; j < colliders.Length; j++)
                {
                    UnityEngine.Object.Destroy(colliders[j]);
                }
                return false;
            }
        }

        // optimize InitMatchScoreUI
        [HarmonyPatch]
        public class ReversePatchYieldForOutro
        {
            //Lets us use the existing YieldForOutro IEnum
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(MatchManager), "YieldForOutro")]
            public static IEnumerator MyCallPrivateMethodStub(MatchManager instance, bool param1)
            {

                throw new System.NotImplementedException("It didn't patch!");
            }
        }

        [HarmonyPatch(typeof(MatchManager), "InitMatchScoreUI")]
        public static class InitMatchScoreUI_Patch
        {
            public static bool Prefix(MatchManager __instance, GameLogic.GameState currentGameState)
            {
                MatchScoreUI prefab = GameManager.GMInstance.assetSystem
        .LoadAsset<GameObject>("PostMatchPoints", "Assets/Prefabs/UI/PostMatchPoints.prefab").GetComponent<MatchScoreUI>();

                __instance.matchScoreUI = UnityEngine.Object.Instantiate(prefab, __instance.canvasContainer.transform, false);

                __instance.matchScoreUI.transform.SetAsFirstSibling();

                __instance.matchScoreUI.Init();
                __instance.matchScoreUI.Populate(currentGameState);

                __instance.StartCoroutine(ReversePatchYieldForOutro.MyCallPrivateMethodStub(__instance, (currentGameState.gameMode == GameState.GameMode.Tutorial)));
                return false;
            }
        }


        // skip running steam callbacks during gameplay - TODO run them but more optimized. This causes a big GC spike once the game ends.
        //[HarmonyPatch(typeof(CallbackDispatcher))]
        //[HarmonyPatch("RunFrame")]
        //public static class Runframe_Patch
        //{
        //    public static bool Prefix(bool isGameServer)
        //    {
        //        if (SceneManager.GetActiveScene().name == "TiledScene")
        //        {
        //            return false;
        //        }
        //        return true;
        //    }
        //}
        //[HarmonyPatch(typeof(CallbackDispatcher))]
        //[HarmonyPatch("RunFrame")]
        //public static class Runframe_Patch
        //{
        //    public static List<string> __preClientCbs = new List<string>();
        //    public static List<string> __preSrvCbs = new List<string>();
        //    public static bool Prefix(bool isGameServer, ref Dictionary<int, List<Callback>> ___m_registeredGameServerCallbacks, ref Dictionary<int, List<Callback>> ___m_registeredCallbacks, out Stopwatch __state)
        //    {
        //        //if(SteamLobbyManager.Instance.GetStatus()("in_game");)
        //        //if (SceneManager.GetActiveScene().name == "TiledScene")
        //        //{
        //        //    __state = new Stopwatch();
        //        //    return false;

        //        //}
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

        //        //if (__state.ElapsedMilliseconds > 4)
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