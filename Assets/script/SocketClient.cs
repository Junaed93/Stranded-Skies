using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SocketClient : MonoBehaviour
{
    public static SocketClient Instance { get; private set; }

    [Header("Connection Settings")]
    public string serverUrl = "ws://localhost:8080/game";

    [Header("References")]
    public GameObject remotePlayerPrefab;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void GameWSConnect(string url);
    [DllImport("__Internal")] private static extern void GameWSSend(string msg);
    [DllImport("__Internal")] private static extern void GameWSClose();
#endif

    private Dictionary<string, RemotePlayerController> remotePlayers = new Dictionary<string, RemotePlayerController>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure GameObject is named "SocketClient" so SendMessage from JS can find it
        gameObject.name = "SocketClient";
    }

    void Start()
    {
        if (GameSession.Instance != null && GameSession.Instance.mode == GameMode.Multiplayer)
        {
            Connect();
        }
    }

    public void Connect()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log($"[SocketClient] Connecting to {serverUrl}...");
        GameWSConnect(serverUrl);
#else
        Debug.Log("[SocketClient] WebSocket only works in WebGL builds.");
#endif
    }

    // Called from JavaScript via SendMessage when WebSocket opens
    public void OnWSOpen(string unused)
    {
        Debug.Log("[SocketClient] Connected! Sending JOIN...");
#if UNITY_WEBGL && !UNITY_EDITOR
        GameWSSend("{\"type\":\"JOIN\"}");
#endif
    }

    // Called from JavaScript via SendMessage when a message arrives
    public void OnWSMessage(string json)
    {
        HandleMessage(json);
    }

    [Serializable]
    class Packet
    {
        public string type;
        public string id;
        public float x;
        public float y;
        public float velX;
        public bool grounded;
    }

    void HandleMessage(string json)
    {
        try
        {
            Packet p = JsonUtility.FromJson<Packet>(json);

            if (p.type == "MOVE")
            {
                if (remotePlayers.ContainsKey(p.id))
                {
                    remotePlayers[p.id].UpdateState(p.x, p.y, p.velX, p.grounded);
                }
                else
                {
                    SpawnRemotePlayer(p.id, p.x, p.y);
                }
            }
            else if (p.type == "LEAVE")
            {
                RemoveRemotePlayer(p.id);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[SocketClient] Parse error: " + e.Message);
        }
    }

    void SpawnRemotePlayer(string id, float x, float y)
    {
        if (remotePlayerPrefab == null)
        {
            Debug.LogWarning("[SocketClient] remotePlayerPrefab is null!");
            return;
        }

        GameObject go = Instantiate(remotePlayerPrefab, new Vector3(x, y, 0), Quaternion.identity);
        go.name = "RemotePlayer_" + id;

        RemotePlayerController rpc = go.AddComponent<RemotePlayerController>();
        rpc.playerId = id;

        remotePlayers.Add(id, rpc);
        Debug.Log($"[SocketClient] Spawned Remote Player: {id}");
    }

    void RemoveRemotePlayer(string id)
    {
        if (remotePlayers.ContainsKey(id))
        {
            Destroy(remotePlayers[id].gameObject);
            remotePlayers.Remove(id);
            Debug.Log($"[SocketClient] Removed Player: {id}");
        }
    }

    public void SendMove(float x, float y, float velX, bool grounded)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string json = $"{{\"type\":\"MOVE\",\"x\":{x},\"y\":{y},\"velX\":{velX},\"grounded\":{(grounded ? "true" : "false")}}}";
        GameWSSend(json);
#endif
    }

    public void SendString(string msg)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameWSSend(msg);
#endif
    }

    void OnDestroy()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameWSClose();
#endif
    }
}
