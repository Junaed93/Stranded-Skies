using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance;

    [Header("References")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private TileBase groundTile;

    [Header("Generation Settings")]
    [SerializeField] private int seed = 0;
    [SerializeField] private int startY = -2;
    [SerializeField] private int generateAheadDistance = 50;

    private Transform player;
    private int lastX;
    private int lastY;

    private bool initialized;
    private bool playerRegistered;

    // =========================
    // UNITY LIFECYCLE
    // =========================

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (groundTilemap == null)
            groundTilemap = GetComponentInChildren<Tilemap>();

        Debug.Log("[WorldGenerator] Awake");
    }

    void Start()
    {
        if (seed == 0)
            seed = System.DateTime.Now.Millisecond + Random.Range(0, 99999);

        InitializeWorld(seed);
    }

    void Update()
    {
        if (!initialized || !playerRegistered) return;

        if (player.position.x + generateAheadDistance > lastX)
        {
            GenerateNextSection();
        }
    }

    // =========================
    // INITIALIZATION
    // =========================

    public void InitializeWorld(int worldSeed)
    {
        if (initialized) return;

        initialized = true;
        seed = worldSeed;

        Random.InitState(seed);
        groundTilemap.ClearAllTiles();

        lastX = 0;
        lastY = startY;

        GenerateStartPlatform();

        Debug.Log($"[WorldGenerator] Initialized with seed {seed}");
    }

    /// <summary>
    /// MUST be called by PlayerSpawner after spawning the local player
    /// </summary>
    public void RegisterPlayer(Transform playerTransform)
    {
        if (playerRegistered) return;

        playerRegistered = true;
        player = playerTransform;

        // Link with EnemySpawner
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.SetTarget(player);
        }

        // Force safe spawn on generated ground
        Vector3 spawnPos = new Vector3(10f, lastY + 2f, 0f);
        player.position = spawnPos;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Debug.Log("[WorldGenerator] Player registered");
    }

    // =========================
    // GENERATION
    // =========================

    private void GenerateStartPlatform()
    {
        SpawnPlatform(-10, lastY, 40);
        lastX = 30;
    }

    private void GenerateNextSection()
    {
        int gap = Random.Range(2, 4);
        int width = Random.Range(4, 7);
        int y = Mathf.Clamp(lastY + Random.Range(-1, 2), -4, 3);

        int startX = lastX + gap;
        SpawnPlatform(startX, y, width);

        // Try to spawn an enemy on this new platform
        if (EnemySpawner.Instance != null)
        {
            float enemyX = startX + (width / 2f);
            Vector3 enemyPos = new Vector3(enemyX, y + 1.5f, 0); 
            Debug.Log($"[WorldGenerator] Requesting enemy spawn at {enemyPos}");
            EnemySpawner.Instance.TrySpawnEnemy(enemyPos, width);
        }
        else
        {
            Debug.LogWarning("[WorldGenerator] EnemySpawner.Instance is NULL. Skipping enemy spawn.");
        }

        lastX = startX + width;
        lastY = y;

        Debug.Log($"[WorldGenerator] Platform generated at X:{startX} Y:{y}");
    }

    private void SpawnPlatform(int startX, int y, int width)
    {
        if (groundTilemap == null || groundTile == null)
        {
            Debug.LogError("[WorldGenerator] Tilemap or Tile is NULL");
            return;
        }

        for (int i = 0; i < width; i++)
        {
            groundTilemap.SetTile(
                new Vector3Int(startX + i, y, 0),
                groundTile
            );
        }
    }
}
