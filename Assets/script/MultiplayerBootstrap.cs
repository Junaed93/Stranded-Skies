using UnityEngine;

/// <summary>
/// MultiplayerBootstrap.cs
/// Runs on scene start to initialize multiplayer infrastructure.
/// Currently a stub - networking will be added later using WebSockets + Spring Boot.
/// </summary>
public class MultiplayerBootstrap : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the GameSession singleton for mode verification")]
    public GameSession gameSession;

    void Start()
    {
        // Verify we are in multiplayer mode
        if (GameSession.Instance == null)
        {
            Debug.LogError("[MultiplayerBootstrap] GameSession.Instance is null! Make sure GameSession exists in the scene.");
            return;
        }

        if (GameSession.Instance.mode != GameMode.Multiplayer)
        {
            Debug.LogWarning("[MultiplayerBootstrap] GameSession.mode is not Multiplayer. Switching to Multiplayer mode.");
            GameSession.Instance.mode = GameMode.Multiplayer;
        }

        Debug.Log("[MultiplayerBootstrap] Multiplayer Ready");

        // =====================================================
        // TODO: Future WebSocket Connection (Stub)
        // =====================================================
        // ConnectToServer();
        // JoinRoom(roomId);
        // =====================================================
    }

    // =====================================================
    // STUB: Socket Connection Methods
    // These will be implemented when WebSocket integration is added.
    // =====================================================

    /// <summary>
    /// Connects to the WebSocket server.
    /// </summary>
    private void ConnectToServer()
    {
        // TODO: Implement WebSocket connection to Spring Boot backend
        // Example: SocketClient.Instance.Connect("ws://localhost:8080/game");
        Debug.Log("[MultiplayerBootstrap] ConnectToServer() - Stub called");
    }

    /// <summary>
    /// Joins a multiplayer room/session.
    /// </summary>
    /// <param name="roomId">The room ID to join</param>
    private void JoinRoom(string roomId)
    {
        // TODO: Send room join request to server
        // Example: SocketClient.Instance.Send(new JoinRoomMessage(roomId));
        Debug.Log($"[MultiplayerBootstrap] JoinRoom({roomId}) - Stub called");
    }
}
