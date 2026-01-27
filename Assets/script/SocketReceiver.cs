using UnityEngine;

public class SocketReceiver : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Parent transform containing all remote player instances")]
    public Transform remotePlayersParent;

    public void OnPlayerStateReceived(string playerId, Vector2 position)
    {
        Debug.Log($"[SocketReceiver] OnPlayerStateReceived({playerId}, {position}) - Stub called");
    }

    public void OnPlayerAttackReceived(string playerId, int attackIndex)
    {
        Debug.Log($"[SocketReceiver] OnPlayerAttackReceived({playerId}, {attackIndex}) - Stub called");
    }

    public void OnDamageConfirmed(string targetId, int damage, int newHealth)
    {
        Debug.Log($"[SocketReceiver] OnDamageConfirmed({targetId}, {damage}, {newHealth}) - Stub called");
    }

    public void OnPlayerJoined(string playerId, Vector2 position)
    {
        Debug.Log($"[SocketReceiver] OnPlayerJoined({playerId}, {position}) - Stub called");
    }

    public void OnPlayerLeft(string playerId)
    {
        Debug.Log($"[SocketReceiver] OnPlayerLeft({playerId}) - Stub called");
    }

    public void OnScoreUpdated(int newScore)
    {
        Debug.Log($"[SocketReceiver] OnScoreUpdated({newScore}) - Stub called");
    }
}
