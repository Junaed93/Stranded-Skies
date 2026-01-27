using UnityEngine;
using System.Collections.Generic;

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

    private bool spawningEnabled = false;

    public void EnableSpawning(bool enable)
    {
        spawningEnabled = enable;
        Debug.Log($"[EnemySpawner] Spawning Enabled: {enable}");
        
        if (enable) nextSpawnTime = Time.time + spawnInterval;
    }

    void Start()
    {
        
        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Ground", "Default");
        }
        else if (groundLayer == LayerMask.GetMask("Ground"))
        {
             groundLayer |= LayerMask.GetMask("Default");
        }
            
        Debug.Log("[EnemySpawner] Initialized (Waiting for Enable)");
    }

    public void SetTarget(Transform playerTransform)
    {
        target = playerTransform;
        Debug.Log("[EnemySpawner] Target manually set to: " + (target != null ? target.name : "NULL"));
    }

    void Update()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("[EnemySpawner] Player found via Tag Search");
            }
            return;
        }

        if (enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] No enemy prefabs assigned!");
            return;
        }

        activeEnemies.RemoveAll(e => e == null);

        
    }

    void SpawnEnemy()
    {
        int enemyIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject prefab = enemyPrefabs[enemyIndex];

        int maxAttempts = 10;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
            float direction = Random.value > 0.5f ? 1f : -1f;
            float spawnX = target.position.x + (distance * direction);

            Vector3? validSpawnPos = FindValidSpawnPosition(spawnX);
            
            if (validSpawnPos.HasValue)
            {
                GameObject enemy = Instantiate(prefab, validSpawnPos.Value, Quaternion.identity, enemiesParent);
                enemy.name = $"Enemy_{activeEnemies.Count}";
                activeEnemies.Add(enemy);

                Debug.Log($"[EnemySpawner] Spawned {prefab.name} at {validSpawnPos.Value}");
                return;
            }
        }
        
        Debug.LogWarning("[EnemySpawner] Could not find valid spawn position after max attempts");
    }

    Vector3? FindValidSpawnPosition(float spawnX)
    {
        Vector2 playerRayStart = new Vector2(target.position.x, target.position.y);
        
        RaycastHit2D hitCheck = Physics2D.Raycast(playerRayStart, Vector2.down, 10f);
        if (hitCheck.collider != null)
        {
        }
        else
        {
             Debug.LogWarning($"[EnemySpawner] Raycast from {playerRayStart} hit NOTHING! Check Z-positions or Colliders.");
             return null;
        }

        float playerGroundY = hitCheck.point.y;
        
        Vector2 spawnRayStart = new Vector2(spawnX, raycastStartHeight);
        RaycastHit2D spawnHit = Physics2D.Raycast(spawnRayStart, Vector2.down, maxRaycastDistance, groundLayer);
        
        if (spawnHit.collider == null)
        {
            spawnHit = Physics2D.Raycast(spawnRayStart, Vector2.down, maxRaycastDistance);
        }

        if (spawnHit.collider == null) return null;
        
        float spawnGroundLevel = spawnHit.point.y;
        
        if (Mathf.Abs(spawnGroundLevel - playerGroundY) > 3f) return null;

        return new Vector3(spawnX, spawnGroundLevel + 0.5f, 0);
    }


    public void TrySpawnEnemy(Vector3 position, int platformWidth)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] TrySpawnEnemy SKIPPED: No enemy prefabs assigned!");
            return;
        }

        if (activeEnemies.Count >= maxEnemies)
        {
            Debug.Log("[EnemySpawner] TrySpawnEnemy SKIPPED: Max enemies reached (" + maxEnemies + ")");
            return;
        }

        int enemyIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject prefab = enemyPrefabs[enemyIndex];

        if (prefab == null)
        {
            Debug.LogError("[EnemySpawner] TrySpawnEnemy ERROR: Prefab at index " + enemyIndex + " is NULL!");
            return;
        }

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity, enemiesParent);
        enemy.name = $"Enemy_Gen_{activeEnemies.Count}";
        activeEnemies.Add(enemy);
        
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
             ai.player = target; 
        }

        Debug.Log($"[EnemySpawner] SUCCESS: Spawned {prefab.name} at {position}. Active count: {activeEnemies.Count}");
    }

    public void OnEnemyKilled(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
    }

    public int GetActiveEnemyCount()
    {
        activeEnemies.RemoveAll(e => e == null);
        return activeEnemies.Count;
    }
}
