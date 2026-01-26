using UnityEngine;
using System;
using System.Collections;

public class FakeServer : MonoBehaviour
{
    public static FakeServer Instance { get; private set; }

    // Events simulating incoming WebSocket messages
    public event Action<int> OnSeedReceived;
    public event Action<int> OnDamageApproved;
    public event Action OnBossSpawnCommand;

    private int killCount = 0;
    private const int KILLS_FOR_BOSS = 5; 

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void Connect()
    {
        Debug.Log("[FakeServer] Connecting...");
        StartCoroutine(SimulateConnection());
    }

    IEnumerator SimulateConnection()
    {
        yield return new WaitForSeconds(1f); // Simulate latency
        Debug.Log("[FakeServer] Connected! Sending Seed.");
        
        // Send pseudo-random seed
        int seed = (int)System.DateTime.Now.Ticks;
        OnSeedReceived?.Invoke(seed);
    }

    public void RequestDamage(int amount, string targetId)
    {
        // validate damage (always valid in mock)
        // Debug.Log($"[FakeServer] Approved damage: {amount} to {targetId}");
        OnDamageApproved?.Invoke(amount); 
    }

    public void ReportEnemyKill()
    {
        killCount++;
        Debug.Log($"[FakeServer] Enemy count: {killCount}/{KILLS_FOR_BOSS}");
        
        if (killCount >= KILLS_FOR_BOSS)
        {
            Debug.Log("[FakeServer] Boss Condition Met! Sending Spwan Command.");
            OnBossSpawnCommand?.Invoke();
            killCount = 0; // Reset
        }
    }
}
