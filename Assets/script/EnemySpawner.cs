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
        
        // AUTO-FIX: If groundLayer is 0 or just "Ground", add "Default" to it
        // This failsafe ensures we hit the collider even if Layer isn't set perfectly
        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Ground", "Default");
            Debug.Log("[EnemySpawner] Auto-set Ground Layer to: Ground + Default");
        }
        else if (groundLayer == LayerMask.GetMask("Ground"))
        {
             groundLayer |= LayerMask.GetMask("Default");
             Debug.Log("[EnemySpawner] Extended Ground Layer to include Default");
        }
            
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
        /* DISABLED TIME-BASED SPAWNING - Now controlled by WorldGenerator only
        if (Time.time >= nextSpawnTime && activeEnemies.Count < maxEnemies)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
        */
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
        // 1. Check if Player is on ground
        Vector2 playerRayStart = new Vector2(target.position.x, target.position.y);
        
        // Debug check: Try to hit ANYTHING below player
        RaycastHit2D hitCheck = Physics2D.Raycast(playerRayStart, Vector2.down, 10f);
        if (hitCheck.collider != null)
        {
             // Debug.Log($"[EnemySpawner] Player is standing above: {hitCheck.collider.name} (Layer: {LayerMask.LayerToName(hitCheck.collider.gameObject.layer)})");
        }
        else
        {
             Debug.LogWarning($"[EnemySpawner] Raycast from {playerRayStart} hit NOTHING! Check Z-positions or Colliders.");
             return null;
        }

        // Use the hit point as the ground reference
        float playerGroundY = hitCheck.point.y;
        
        // 2. Check Spawn Point
        Vector2 spawnRayStart = new Vector2(spawnX, raycastStartHeight);
        RaycastHit2D spawnHit = Physics2D.Raycast(spawnRayStart, Vector2.down, maxRaycastDistance, groundLayer);
        
        // If strict ground check fails, try ALL layers
        if (spawnHit.collider == null)
        {
            spawnHit = Physics2D.Raycast(spawnRayStart, Vector2.down, maxRaycastDistance);
        }

        if (spawnHit.collider == null) return null; // Still nothing? Give up.
        
        float spawnGroundLevel = spawnHit.point.y;
        
        // 3. Height Tolerance Check
        if (Mathf.Abs(spawnGroundLevel - playerGroundY) > 3f) return null;

        return new Vector3(spawnX, spawnGroundLevel + 0.5f, 0);
    }


    /// <summary>
    /// Tries to spawn an enemy on a newly generated platform.
    /// Called directly by WorldGenerator.
    /// </summary>
    public void TrySpawnEnemy(Vector3 position, int platformWidth)
    {
        if (enemyPrefabs.Length == 0) return;
        if (activeEnemies.Count >= maxEnemies) return;

        // FORCE SPAWN (Removed 50% chance for debugging)
        // if (Random.value > 0.5f) return;

        int enemyIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject prefab = enemyPrefabs[enemyIndex];

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity, enemiesParent);
        enemy.name = $"Enemy_Gen_{activeEnemies.Count}";
        activeEnemies.Add(enemy);
        
        // [FIX] Inject dependency immediately - No tag searching race conditions
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
             ai.player = target; 
        }

        Debug.Log($"[EnemySpawner] Generated enemy at {position}");
    }

    /// <summary>
    /// Called when an enemy is killed. Updates tracking.
    /// </summary>
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
