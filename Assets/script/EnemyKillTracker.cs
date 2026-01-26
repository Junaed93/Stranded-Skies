using UnityEngine;

/// <summary>
/// EnemyKillTracker.cs
/// Tracks total enemy kills in the multiplayer session.
/// Used ONLY in Multiplayer.scene.
/// </summary>
public class EnemyKillTracker : MonoBehaviour
{
    public static EnemyKillTracker Instance { get; private set; }

    [Header("Kill Tracking")]
    [Tooltip("Total enemies killed this session")]
    public int totalKills = 0;

    /// <summary>
    /// Event fired when an enemy is killed. Passes the new total kill count.
    /// </summary>
    public System.Action<int> OnEnemyKilled;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Registers an enemy kill and notifies listeners.
    /// </summary>
    public void RegisterKill()
    {
        totalKills++;
        Debug.Log($"[EnemyKillTracker] Enemy killed! Total: {totalKills}");
        OnEnemyKilled?.Invoke(totalKills);
    }

    /// <summary>
    /// Resets the kill counter.
    /// </summary>
    public void ResetKills()
    {
        totalKills = 0;
        Debug.Log("[EnemyKillTracker] Kill counter reset");
    }

    /// <summary>
    /// Returns the current kill count.
    /// </summary>
    public int GetKillCount()
    {
        return totalKills;
    }
}
