using UnityEngine;

/// <summary>
/// SocketClient.cs
/// STUB - WebSocket client for connecting to the Spring Boot backend.
/// This script will be implemented later when networking is added.
/// </summary>
public class SocketClient : MonoBehaviour
{
    public static SocketClient Instance { get; private set; }

    [Header("Connection Settings")]
    [Tooltip("WebSocket server URL (e.g., ws://localhost:8080/game)")]
    public string serverUrl = "ws://localhost:8080/game";

    [Header("Connection State")]
    public bool isConnected = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // =====================================================
    // STUB: Connection Methods
    // =====================================================

    /// <summary>
    /// Connects to the WebSocket server.
    /// </summary>
    public void Connect()
    {
        // TODO: Implement WebSocket connection
        Debug.Log($"[SocketClient] Connect() to {serverUrl} - Stub called");
        isConnected = false; // Will be true when actually connected
    }

    /// <summary>
    /// Disconnects from the WebSocket server.
    /// </summary>
    public void Disconnect()
    {
        // TODO: Implement WebSocket disconnection
        Debug.Log("[SocketClient] Disconnect() - Stub called");
        isConnected = false;
    }

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="message">JSON message string to send</param>
    public void Send(string message)
    {
        // TODO: Implement message sending
        Debug.Log($"[SocketClient] Send({message}) - Stub called");
    }

    void OnDestroy()
    {
        Disconnect();
    }
}
