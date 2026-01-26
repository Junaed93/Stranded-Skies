using UnityEngine;

/// <summary>
/// BossSpawnManager.cs
/// Spawns a boss after N enemy kills.
/// NO boss walls - boss spawns naturally into the world.
/// Used ONLY in Multiplayer.scene.
/// </summary>
public class BossSpawnManager : MonoBehaviour
{
    public static BossSpawnManager Instance { get; private set; }

    [Header("Boss Settings")]
    [Tooltip("Boss prefabs to spawn (randomly selected)")]
    public GameObject[] bossPrefabs;

    [Tooltip("Number of kills required to spawn a boss")]
    public int killsToSpawnBoss = 1;

    [Tooltip("Distance ahead of player to spawn boss")]
    public float bossSpawnDistance = 15f;

    [Tooltip("Y position for boss spawn (fallback)")]
    public float bossSpawnY = -1f;

    [Tooltip("Layer mask for ground detection")]
    public LayerMask groundLayer;

    [Tooltip("Height to start raycast from")]
    public float raycastStartHeight = 20f;

    [Tooltip("Maximum raycast distance")]
    public float maxRaycastDistance = 50f;

    [Header("References")]
    [Tooltip("Transform to track for spawning (usually the player)")]
    public Transform target;

    [Tooltip("Parent transform for bosses")]
    public Transform bossesParent;

    // State
    private int lastBossSpawnKillCount = 0;
    private bool bossActive = false;
    private GameObject currentBoss;

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
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        // Initialize ground layer if not set
        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");

        // Subscribe to kill tracker
        if (EnemyKillTracker.Instance != null)
        {
            EnemyKillTracker.Instance.OnEnemyKilled += OnKillCountChanged;
        }
    }

    void OnDestroy()
    {
        if (EnemyKillTracker.Instance != null)
        {
            EnemyKillTracker.Instance.OnEnemyKilled -= OnKillCountChanged;
        }
    }

    /// <summary>
    /// Called when the kill count changes.
    /// </summary>
    /// <param name="totalKills">The new total kill count</param>
    void OnKillCountChanged(int totalKills)
    {
        // Check if we should spawn a boss
        int killsSinceLastBoss = totalKills - lastBossSpawnKillCount;

        if (!bossActive && killsSinceLastBoss >= killsToSpawnBoss)
        {
            SpawnBoss();
            lastBossSpawnKillCount = totalKills;
        }
    }

    /// <summary>
    /// Spawns a boss ahead of the player.
    /// </summary>
    void SpawnBoss()
    {
        if (bossPrefabs.Length == 0 || target == null) return;

        // Pick a random boss
        int bossIndex = 0;
        if (SeedManager.Instance != null)
        {
            bossIndex = SeedManager.Instance.NextInt(0, bossPrefabs.Length);
        }
        else
        {
            bossIndex = Random.Range(0, bossPrefabs.Length);
        }

        GameObject prefab = bossPrefabs[bossIndex];

        // Try to find a valid spawn position (check both directions)
        Vector3? validSpawnPos = FindValidBossSpawnPosition();
        
        if (!validSpawnPos.HasValue)
        {
            Debug.LogWarning("[BossSpawnManager] Could not find valid spawn position for boss!");
            return;
        }

        currentBoss = Instantiate(prefab, validSpawnPos.Value, Quaternion.identity, bossesParent);
        currentBoss.name = $"Boss_{prefab.name}";
        bossActive = true;

        Debug.Log($"[BossSpawnManager] Boss spawned: {prefab.name} at {validSpawnPos.Value}");

        // Subscribe to boss death (if it has IDamageable)
        // This would need to be handled by the boss script calling OnBossDefeated
    }

    /// <summary>
    /// Finds a valid spawn position for the boss by checking ground connectivity.
    /// </summary>
    Vector3? FindValidBossSpawnPosition()
    {
        // Find what ground level the player is on
        Vector2 playerRayStart = new Vector2(target.position.x, target.position.y + 1f);
        RaycastHit2D playerGroundHit = Physics2D.Raycast(playerRayStart, Vector2.down, 10f, groundLayer);
        
        // Fallback to Default layer if Ground layer fails
        if (playerGroundHit.collider == null)
            playerGroundHit = Physics2D.Raycast(playerRayStart, Vector2.down, 10f);

        if (playerGroundHit.collider == null)
        {
            Debug.LogWarning($"[BossSpawnManager] Player not on ground at {playerRayStart}! Boss cannot spawn.");
            return null;
        }
        
        float playerGroundY = playerGroundHit.point.y;
        
        // Try different distances and both directions
        float[] distancesToTry = { bossSpawnDistance, bossSpawnDistance * 0.75f, bossSpawnDistance * 0.5f, bossSpawnDistance * 0.25f };
        float[] directions = { target.localScale.x > 0 ? 1f : -1f, target.localScale.x > 0 ? -1f : 1f };
        
        foreach (float dir in directions)
        {
            foreach (float dist in distancesToTry)
            {
                float spawnX = target.position.x + (dist * dir);
                
                // Raycast at spawn X position
                Vector2 spawnRayStart = new Vector2(spawnX, raycastStartHeight);
                RaycastHit2D spawnHit = Physics2D.Raycast(spawnRayStart, Vector2.down, maxRaycastDistance, groundLayer);
                
                if (spawnHit.collider == null) continue;
                
                float spawnGroundLevel = spawnHit.point.y;
                
                // Check height tolerance
                float heightTolerance = 3f;
                if (Mathf.Abs(spawnGroundLevel - playerGroundY) > heightTolerance) continue;
                
                // Verify continuous ground between player and spawn point
                // SKIP strict continuity check for platformer mode (gaps are expected)
                // if (!IsGroundContinuous(target.position.x, spawnX, playerGroundY)) continue;
                
                // Valid position found
                return new Vector3(spawnX, spawnGroundLevel + 0.5f, 0);
            }
        }
        
        return null;
    }

    /// <summary>
    /// Checks if there's continuous ground between two X positions.
    /// </summary>
    bool IsGroundContinuous(float startX, float endX, float groundY)
    {
        float checkInterval = 2f;
        float minX = Mathf.Min(startX, endX);
        float maxX = Mathf.Max(startX, endX);
        
        for (float checkX = minX; checkX <= maxX; checkX += checkInterval)
        {
            Vector2 checkRayStart = new Vector2(checkX, groundY + 2f);
            RaycastHit2D checkHit = Physics2D.Raycast(checkRayStart, Vector2.down, 5f, groundLayer);
            
            if (checkHit.collider == null)
            {
                // Gap detected!
                return false;
            }
        }
        
        return true;
    }

    /// <summary>
    /// Called when a boss is defeated.
    /// </summary>
    public void OnBossDefeated()
    {
        bossActive = false;
        currentBoss = null;
        Debug.Log("[BossSpawnManager] Boss defeated! Next boss will spawn after more kills.");
    }

    /// <summary>
    /// Returns true if a boss is currently active.
    /// </summary>
    public bool IsBossActive()
    {
        // Clean up if boss was destroyed
        if (bossActive && currentBoss == null)
        {
            bossActive = false;
        }
        return bossActive;
    }
}
