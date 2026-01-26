using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// WorldGenerator.cs
/// Scan-and-fill infinite world generation.
/// Fills exact gaps in the world to ensure continuous ground.
/// Used ONLY in Multiplayer.scene.
/// </summary>
public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance { get; private set; }

    [Header("Generation Settings")]
    [Tooltip("Radius around player to ensure ground exists")]
    public int generateRadius = 60;

    [Tooltip("How often to run the generator (seconds)")]
    public float updateInterval = 0.5f;

    [Header("Ground Settings")]
    [Tooltip("The ground tile to paint")]
    public TileBase groundTile;

    [Tooltip("Ground Y position (in tiles)")]
    public int groundLevel = -1;

    [Tooltip("How many tiles deep the ground should be")]
    public int groundDepth = 3;

    [Tooltip("If true, detects existing ground height on Start")]
    public bool autoDetectGroundY = true;

    [Header("References")]
    [Tooltip("The Tilemap to paint ground tiles on")]
    public Tilemap groundTilemap;

    [Tooltip("Transform to track (usually the player)")]
    public Transform target;

    private float nextUpdateTime;

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
        // Safety Checks
        if (groundTile == null) Debug.LogError("[WorldGenerator] CRITICAL: Ground Tile is missing! Assign it in Inspector.");
        if (groundTilemap == null) Debug.LogError("[WorldGenerator] CRITICAL: Ground Tilemap is missing! Assign it in Inspector.");

        // Auto-detect ground level from existing tiles
        // Scans a range around X=0 to avoid missing the ground if X=0 is empty
        if (autoDetectGroundY && groundTilemap != null)
        {
            groundTilemap.CompressBounds();
            BoundsInt bounds = groundTilemap.cellBounds;
            
            bool found = false;
            
            // Scan X range from -20 to 20 to find ground
            int scanRange = 20;
            for (int x = -scanRange; x <= scanRange; x++)
            {
                // Scan from top down to find top-most ground tile
                int startY = Mathf.Min(10, bounds.yMax); 
                int endY = Mathf.Max(-20, bounds.yMin);

                for (int y = startY; y >= endY; y--)
                {
                    if (groundTilemap.HasTile(new Vector3Int(x, y, 0)))
                    {
                        groundLevel = y;
                        found = true;
                        Debug.Log($"[WorldGenerator] Auto-detected ground level at Y={groundLevel} (found at x={x})");
                        goto GroundFound;
                    }
                }
            }
            
            GroundFound:
            if (!found)
            {
                Debug.LogWarning($"[WorldGenerator] Could not auto-detect ground in range x=-{scanRange} to {scanRange}.");
                
                // Fallback: Use player position if available
                if (target != null)
                {
                    // Assuming player pivot is at feet/center, ground is usually 1 unit below
                    groundLevel = Mathf.FloorToInt(target.position.y - 1.5f); 
                    Debug.Log($"[WorldGenerator] FORCING ground level to Player Y ({target.position.y}) - 1.5 => Y={groundLevel}");
                }
                else
                {
                    Debug.LogWarning($"[WorldGenerator] Using default groundLevel: Y={groundLevel}");
                }
            }
        }
        else if (groundTilemap != null && target != null && !autoDetectGroundY)
        {
             // If auto-detect is OFF, still try to align with player just in case default is wrong
             // Uncomment if you want this behavior:
             // groundLevel = Mathf.FloorToInt(target.position.y - 1f);
        }

        // Initial generation around origin
        ScanAndFill(0f);
    }

    void Update()
    {
        // Try to find player if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        // Generate periodically around player
        if (target != null && Time.time >= nextUpdateTime)
        {
            ScanAndFill(target.position.x);
            nextUpdateTime = Time.time + updateInterval;
        }
    }

    /// <summary>
    /// Scans the area around center X and fills any empty ground tiles.
    /// </summary>
    void ScanAndFill(float centerX)
    {
        if (groundTile == null || groundTilemap == null) return;

        int startX = Mathf.FloorToInt(centerX) - generateRadius;
        int endX = Mathf.FloorToInt(centerX) + generateRadius;

        for (int x = startX; x <= endX; x++)
        {
            // Check surface tile
            Vector3Int surfacePos = new Vector3Int(x, groundLevel, 0);

            // If TOP tile is missing, we need to fill this column
            // But we must NOT overwrite existing manual tiles
            // Simple check: Is the spot empty?
            if (!groundTilemap.HasTile(surfacePos))
            {
                FillColumn(x);
            }
        }
    }

    void FillColumn(int x)
    {
        for (int y = 0; y < groundDepth; y++)
        {
            int tileY = groundLevel - y;
            Vector3Int pos = new Vector3Int(x, tileY, 0);

            // Double check we aren't overwriting anything
            if (!groundTilemap.HasTile(pos))
            {
                groundTilemap.SetTile(pos, groundTile);
            }
        }
    }

    public void ResetWorld()
    {
        if (groundTilemap != null)
        {
            groundTilemap.ClearAllTiles();
        }
        ScanAndFill(0f);
    }
}
