using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// EnemySpawner.cs
/// Randomly spawns enemies based on seed and player position.
/// Used ONLY in Multiplayer.scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Enemy Prefabs")]
    [Tooltip("Array of enemy prefabs to spawn")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("Minimum distance from player to spawn enemies")]
    public float minSpawnDistance = 5f;

    [Tooltip("Maximum distance from player to spawn enemies")]
    public float maxSpawnDistance = 12f;

    [Tooltip("Time between spawn attempts")]
    public float spawnInterval = 5f;

    [Tooltip("Maximum enemies alive at once")]
    public int maxEnemies = 10;

    [Tooltip("Y position for ground-level spawns (fallback)")]
    public float spawnGroundY = -1f;

    [Tooltip("Layer mask for ground detection")]
    public LayerMask groundLayer;

    [Tooltip("Height to start raycast from")]
    public float raycastStartHeight = 20f;

    [Tooltip("Maximum raycast distance")]
    public float maxRaycastDistance = 50f;

    [Header("References")]
    [Tooltip("Transform to track for spawning (usually the player)")]
    public Transform target;

    [Tooltip("Parent transform for spawned enemies")]
    public Transform enemiesParent;

    // Tracking
    private float nextSpawnTime;
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
        
        // Initialize ground layer if not set
        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");
            
        Debug.Log("[EnemySpawner] Started");
    }

    void Update()
    {
        // Continuously search for player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("[EnemySpawner] Player found, starting to spawn enemies");
            }
            return;
        }

        if (enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] No enemy prefabs assigned!");
            return;
        }

        // Clean up destroyed enemies
        activeEnemies.RemoveAll(e => e == null);

        // Spawn if conditions are met
        if (Time.time >= nextSpawnTime && activeEnemies.Count < maxEnemies)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    /// <summary>
    /// Spawns an enemy at a random position around the player.
    /// </summary>
    void SpawnEnemy()
    {
        // Pick a random enemy type
        int enemyIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject prefab = enemyPrefabs[enemyIndex];

        // Try multiple spawn attempts to find valid connected ground
        int maxAttempts = 10;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Calculate spawn X position
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
            float direction = Random.value > 0.5f ? 1f : -1f;
            float spawnX = target.position.x + (distance * direction);

            // Check if spawn position has valid connected ground
            Vector3? validSpawnPos = FindValidSpawnPosition(spawnX);
            
            if (validSpawnPos.HasValue)
            {
                // Instantiate enemy
                GameObject enemy = Instantiate(prefab, validSpawnPos.Value, Quaternion.identity, enemiesParent);
                enemy.name = $"Enemy_{activeEnemies.Count}";
                activeEnemies.Add(enemy);

                Debug.Log($"[EnemySpawner] Spawned {prefab.name} at {validSpawnPos.Value}");
                return;
            }
        }
        
        Debug.LogWarning("[EnemySpawner] Could not find valid spawn position after max attempts");
    }

    /// <summary>
    /// Finds a valid spawn position by checking ground connectivity.
    /// Returns null if no valid position found.
    /// </summary>
    Vector3? FindValidSpawnPosition(float spawnX)
    {
        // First, find what ground level the player is on
        Vector2 playerRayStart = new Vector2(target.position.x, target.position.y + 1f);
        RaycastHit2D playerGroundHit = Physics2D.Raycast(playerRayStart, Vector2.down, 5f, groundLayer);
        
        if (playerGroundHit.collider == null)
        {
            Debug.LogWarning("[EnemySpawner] Player not on ground!");
            return null;
        }
        
        float playerGroundY = playerGroundHit.point.y;
        
        // Now raycast at spawn X position
        Vector2 spawnRayStart = new Vector2(spawnX, raycastStartHeight);
        RaycastHit2D spawnHit = Physics2D.Raycast(spawnRayStart, Vector2.down, maxRaycastDistance, groundLayer);
        
        if (spawnHit.collider == null)
        {
            // No ground at spawn position
            return null;
        }
        
        float spawnGroundLevel = spawnHit.point.y;
        
        // Check if spawn ground is at similar height to player ground (within tolerance)
        // This prevents spawning across gaps onto different platform levels
        float heightTolerance = 3f;
        if (Mathf.Abs(spawnGroundLevel - playerGroundY) > heightTolerance)
        {
            // Ground is at a very different height - likely a different platform
            return null;
        }
        
        // Verify there's continuous ground between player and spawn point
        // Check at intervals to detect gaps
        float checkInterval = 2f;
        float startX = Mathf.Min(target.position.x, spawnX);
        float endX = Mathf.Max(target.position.x, spawnX);
        
        for (float checkX = startX; checkX <= endX; checkX += checkInterval)
        {
            Vector2 checkRayStart = new Vector2(checkX, playerGroundY + 2f);
            RaycastHit2D checkHit = Physics2D.Raycast(checkRayStart, Vector2.down, 5f, groundLayer);
            
            if (checkHit.collider == null)
            {
                // Gap detected!
                return null;
            }
        }
        
        // Valid spawn position found
        return new Vector3(spawnX, spawnGroundLevel + 0.5f, 0);
    }


    /// <summary>
    /// Called when an enemy is killed. Updates tracking.
    /// </summary>
    /// <param name="enemy">The enemy that was killed</param>
    public void OnEnemyKilled(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
    }

    /// <summary>
    /// Returns the current number of active enemies.
    /// </summary>
    public int GetActiveEnemyCount()
    {
        activeEnemies.RemoveAll(e => e == null);
        return activeEnemies.Count;
    }
}
