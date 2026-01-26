using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance { get; private set; }

    [Header("References")]
    public Tilemap groundTilemap;
    public TileBase[] groundTiles;
    public Transform player; 

    [Header("Settings")]
    public int seed = 12345;
    
    private int lastX = 0;
    private int lastY = -2;

    // Awake removed (Merged below to avoid duplicate error)

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Auto-setup Composite Collider for smoother physics
        if (groundTilemap != null)
        {
            // Ensure TilemapCollider2D exists
            TilemapCollider2D tmCollider = groundTilemap.GetComponent<TilemapCollider2D>();
            if (tmCollider == null) tmCollider = groundTilemap.gameObject.AddComponent<TilemapCollider2D>();

            // Ensure CompositeCollider2D exists
            CompositeCollider2D compCollider = groundTilemap.GetComponent<CompositeCollider2D>();
            if (compCollider == null) compCollider = groundTilemap.gameObject.AddComponent<CompositeCollider2D>();

            // Configure
            groundTilemap.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            tmCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
        }

        SetSeed(seed);
    }

    void Update() {
        if (player == null)
        {
             GameObject p = GameObject.FindGameObjectWithTag("Player");
             if (p != null) player = p.transform;
             return;
        }

        // AGGRESSIVE GENERATION
        // If player is within 60 units of the edge, BUILD MORE.
        // Don't wait.
        if (player.position.x > lastX - 60) 
        {
            GenerateNextSection();
        }
    }

    public void SetSeed(int newSeed) {
        seed = newSeed;
        if (groundTilemap != null) groundTilemap.ClearAllTiles();
        
        lastX = 0; 
        
        // Sync starting height with player position if possible
        if (player != null)
        {
            // Use -1.5 to be safe under feet
            lastY = Mathf.FloorToInt(player.position.y - 1.5f);
            Debug.Log($"[WorldGenerator] Synced generation height to Player Y: {lastY}");
        }
        else
        {
            lastY = -2;
        }

        Random.InitState(seed);
        
        // Ensure we have tiles
        if (groundTiles == null || groundTiles.Length == 0)
        {
            Debug.LogError("No ground tiles assigned in WorldGenerator!");
            return;
        }

        // Generate SAFE Start Zone: Start 15 units BEHIND player, go 40 units total
        // Note: If player is at 0, startX is -15. Center is roughly +5.
        // We force alignment to ensure no gap falls.
        int startX = Mathf.FloorToInt(player != null ? player.position.x : 0) - 15;
        int safeWidth = 40;
        
        SpawnPlatform(startX, lastY, safeWidth); 
        lastX = startX + safeWidth;

        // CRITICAL FIX: Teleport player to valid ground immediately
        if (player != null)
        {
            float safeX = startX + (safeWidth / 2f);
            float safeY = lastY + 2f; // +2 to be safely above
            player.position = new Vector3(safeX, safeY, 0);
            
            // Kill any initial velocity
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            
            Debug.Log($"[WorldGenerator] FORCE-TELEPORTED Player to Safe Start: {player.position}");
        }
    }

    void GenerateNextSection() {
        // Deterministic generation based on seed and position
        Random.InitState(seed + lastX); 
        int gap = Random.Range(2, 4); // Smaller gaps (was 2,5)
        int width = 5; 
        int y = Mathf.Clamp(lastY + Random.Range(-2, 3), -5, 2); 

        SpawnPlatform(lastX + gap, y, width);
        Debug.Log($"[WorldGenerator] Generated platform at X:{lastX + gap} Y:{y}");
        
        // Call Enemy Spawner during world gen
        float platformCenterX = (lastX + gap) + (width / 2f);
        Vector3 spawnPos = new Vector3(platformCenterX, y + 1, 0); 
        
        EnemySpawner.Instance?.TrySpawnEnemy(spawnPos, width);
        
        lastX += gap + width;
        lastY = y;
    }

    void SpawnPlatform(int startX, int y, int width) {
        if (groundTiles.Length == 0) return;

        for (int i = 0; i < width; i++)
        {
            groundTilemap.SetTile(new Vector3Int(startX + i, y, 0), groundTiles[0]);
        }
    }

    /// <summary>
    /// Returns a guaranteed safe spawn point on the first platform.
    /// Used by PlayerSpawner to prevent spawning in gaps/void.
    /// </summary>
    public Vector3 GetSafeSpawnPosition()
    {
        // Increased safety height to prevent clipping
        return new Vector3(lastX - 20, lastY + 5f, 0);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        
        // Randomize Seed on Awake (Unified Logic)
        if (seed == 0 || seed == 12345) 
        {
            seed = (int)System.DateTime.Now.Ticks;
            Debug.Log($"[WorldGenerator] Randomized Seed: {seed}");
        }
    }

    void OnGUI()
    {
        if (player == null) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.yellow;
        
        float threshold = lastX - 45;
        string status = player.position.x > threshold ? "GENERATING NOW" : "Waiting for move";
        
        GUI.Label(new Rect(10, 10, 500, 100), $"Player X: {player.position.x:F1}\nThreshold: {threshold} (LastX: {lastX})\nStatus: {status}", style);
    }
}
