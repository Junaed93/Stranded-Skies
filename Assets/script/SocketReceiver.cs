using UnityEngine;

/// <summary>
/// SocketReceiver.cs
/// STUB - Handles incoming WebSocket messages from the Spring Boot backend.
/// This script will be implemented later when networking is added.
/// </summary>
public class SocketReceiver : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Parent transform containing all remote player instances")]
    public Transform remotePlayersParent;

    // =====================================================
    // STUB: Message Handling Methods
    // These will be called when actual WebSocket messages are received.
    // =====================================================

    /// <summary>
    /// Called when a player state update is received from the server.
    /// </summary>
    /// <param name="playerId">ID of the player</param>
    /// <param name="position">New position of the player</param>
    public void OnPlayerStateReceived(string playerId, Vector2 position)
    {
        // TODO: Find the remote player and update their state
        // RemotePlayerController controller = FindRemotePlayer(playerId);
        // if (controller != null) controller.ApplyState(position);
        Debug.Log($"[SocketReceiver] OnPlayerStateReceived({playerId}, {position}) - Stub called");
    }

    /// <summary>
    /// Called when a player attack message is received from the server.
    /// </summary>
    /// <param name="playerId">ID of the attacking player</param>
    /// <param name="attackIndex">Attack combo index</param>
    public void OnPlayerAttackReceived(string playerId, int attackIndex)
    {
        // TODO: Find the remote player and trigger attack animation
        Debug.Log($"[SocketReceiver] OnPlayerAttackReceived({playerId}, {attackIndex}) - Stub called");
    }

    /// <summary>
    /// Called when a damage event is confirmed by the server.
    /// </summary>
    /// <param name="targetId">ID of the target that received damage</param>
    /// <param name="damage">Amount of damage</param>
    /// <param name="newHealth">New health value after damage</param>
    public void OnDamageConfirmed(string targetId, int damage, int newHealth)
    {
        // TODO: Apply damage to the target (could be local player or enemy)
        Debug.Log($"[SocketReceiver] OnDamageConfirmed({targetId}, {damage}, {newHealth}) - Stub called");
    }

    /// <summary>
    /// Called when a player joins the room.
    /// </summary>
    /// <param name="playerId">ID of the new player</param>
    /// <param name="position">Spawn position of the new player</param>
    public void OnPlayerJoined(string playerId, Vector2 position)
    {
        // TODO: Spawn a new remote player
        Debug.Log($"[SocketReceiver] OnPlayerJoined({playerId}, {position}) - Stub called");
    }

    /// <summary>
    /// Called when a player leaves the room.
    /// </summary>
    /// <param name="playerId">ID of the leaving player</param>
    public void OnPlayerLeft(string playerId)
    {
        // TODO: Remove the remote player instance
        Debug.Log($"[SocketReceiver] OnPlayerLeft({playerId}) - Stub called");
    }

    /// <summary>
    /// Called when score is updated by the server.
    /// </summary>
    /// <param name="newScore">The new score value</param>
    public void OnScoreUpdated(int newScore)
    {
        // TODO: Update the local score display
        // ScoreManager.Instance.ApplyScore(newScore - ScoreManager.Instance.GetScore());
        Debug.Log($"[SocketReceiver] OnScoreUpdated({newScore}) - Stub called");
    }
}
