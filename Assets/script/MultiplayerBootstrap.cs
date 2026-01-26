using UnityEngine;

/// <summary>
/// MultiplayerBootstrap.cs
/// Runs on scene start to initialize multiplayer infrastructure.
/// Currently a stub - networking will be added later using WebSockets + Spring Boot.
/// </summary>
public class MultiplayerBootstrap : MonoBehaviour
{
    [Header("References")]
    public WorldGenerator worldGenerator;
    public PlayerSpawner playerSpawner;
    public EnemySpawner enemySpawner;
    public BossSpawnManager bossSpawner;

    void Start()
    {
        Debug.Log("[MultiplayerBootstrap] Initializing...");

        // 1. Force Mode
        if (GameModeManager.Instance != null)
             GameModeManager.Instance.currentMode = GameMode.Multiplayer;

        // 2. Initialize Server
        if (FakeServer.Instance == null)
             gameObject.AddComponent<FakeServer>();

        // 3. Subscribe to Events
        FakeServer.Instance.OnSeedReceived += HandleSeedReceived;
        FakeServer.Instance.OnBossSpawnCommand += HandleBossSpawn;

        // 4. Connect
        FakeServer.Instance.Connect();
    }

    void HandleSeedReceived(int seed)
    {
        Debug.Log($"[MultiplayerBootstrap] Seed Received: {seed}. Starting World Gen.");
        
        // 5. Start World Gen
        if (worldGenerator != null)
        {
            worldGenerator.InitializeWorld(seed, OnWorldReady);
        }
    }

    void OnWorldReady()
    {
        Debug.Log("[MultiplayerBootstrap] World Ready. Spawning Player.");

        // 6. Spawn Player logic
        if (playerSpawner != null && worldGenerator != null)
        {
            Vector3 spawnPos = worldGenerator.GetSafeSpawnPosition();
            playerSpawner.SpawnPlayerAt(spawnPos);
        }

        // 7. Enable Enemy Spawning
        if (enemySpawner != null)
        {
            enemySpawner.EnableSpawning(true);
        }
    }

    void HandleBossSpawn()
    {
        Debug.Log("[MultiplayerBootstrap] Boss Command Received. Spawning Boss.");
        if (bossSpawner != null && playerSpawner.localPlayerInstance != null)
        {
            bossSpawner.SpawnBoss(playerSpawner.localPlayerInstance.transform);
        }
    }
}
