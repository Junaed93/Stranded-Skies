using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SocketClient : MonoBehaviour
{
    public static SocketClient Instance { get; private set; }

    [Header("Connection Settings")]
    [Tooltip("WebSocket server URL (e.g., ws://localhost:3000/game)")]
    public string serverUrl = "ws://localhost:8080/game";

    [Header("References")]
    public GameObject remotePlayerPrefab; // ASSIGN IN INSPECTOR

    private ClientWebSocket _ws;
    private CancellationTokenSource _cts;
    
    // Remote Players Map: ID -> Controller
    private Dictionary<string, RemotePlayerController> remotePlayers = new Dictionary<string, RemotePlayerController>();

    // Main Thread Queue
    private Queue<string> messageQueue = new Queue<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (GameSession.Instance.mode == GameMode.Multiplayer)
        {
            Connect();
        }
    }

    public async void Connect()
    {
        if (_ws != null && _ws.State == WebSocketState.Open) return;

        _ws = new ClientWebSocket();
        _cts = new CancellationTokenSource();

        try
        {
            Debug.Log($"[SocketClient] Connecting to {serverUrl}...");
            await _ws.ConnectAsync(new Uri(serverUrl), _cts.Token);
            Debug.Log("[SocketClient] Connected!");
            
            // Start Receiving
            ReceiveLoop();
            
            // Send Join
            SendJson(new { type = "JOIN" });
        }
        catch (Exception e)
        {
            Debug.LogError($"[SocketClient] Connection Error: {e.Message}");
        }
    }

    private async void ReceiveLoop()
    {
        var buffer = new byte[1024 * 4];

        while (_ws.State == WebSocketState.Open && !_cts.IsCancellationRequested)
        {
            try
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    lock (messageQueue)
                    {
                        messageQueue.Enqueue(msg);
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
            }
            catch (Exception) { break; }
        }
    }

    void Update()
    {
        // Process Main Thread Queue
        lock (messageQueue)
        {
            while (messageQueue.Count > 0)
            {
                string msg = messageQueue.Dequeue();
                HandleMessage(msg);
            }
        }
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
            Debug.LogWarning("Errors parsing msg: " + e.Message);
        }
    }

    void SpawnRemotePlayer(string id, float x, float y)
    {
        if (remotePlayerPrefab == null) return;
        
        GameObject go = Instantiate(remotePlayerPrefab, new Vector3(x, y, 0), Quaternion.identity);
        go.name = "RemotePlayer_" + id;
        RemotePlayerController rpc = go.AddComponent<RemotePlayerController>(); // Ensure component exists
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

    public async void SendJson(object data)
    {
        if (_ws == null || _ws.State != WebSocketState.Open) return;

        string json = JsonUtility.ToJson(data); // Simple Unity JSON
        // Note: For anonymous objects, JsonUtility needs a wrapper or manual string construction.
        // For simplicity in this demo, assumes 'data' is a proper serializable class.
        // If sending raw string:
        
        // Quick Fix for Anonymous 'JOIN'
        if (json == "{}") json = "{\"type\":\"JOIN\"}"; 

        byte[] bytes = Encoding.UTF8.GetBytes(json);
        await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
    
    // Explicit Move Send
    public void SendMove(float x, float y, float velX, bool grounded)
    {
        // Manual JSON construction for speed/simplicity without extensive DTOs
        string json = $"{{\"type\":\"MOVE\",\"x\":{x},\"y\":{y},\"velX\":{velX},\"grounded\":{(grounded?"true":"false")}}}";
        SendString(json);
    }
    
    public async void SendString(string msg)
    {
        if (_ws == null || _ws.State != WebSocketState.Open) return;
        byte[] bytes = Encoding.UTF8.GetBytes(msg);
        await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    void OnDestroy()
    {
        if (_cts != null) _cts.Cancel();
        if (_ws != null) _ws.Dispose();
    }
}
