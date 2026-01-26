using UnityEngine;
using UnityEngine.Tilemaps;


public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance { get; private set; }

    [Header("Generation Settings")]
    [Tooltip("Radius around player to ensure ground exists")]
    public int generateRadius = 40;

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
        // Auto-detect ground level from existing tiles
        if (autoDetectGroundY && groundTilemap != null)
        {
            groundTilemap.CompressBounds();
            BoundsInt bounds = groundTilemap.cellBounds;
            
            bool found = false;
            // Scan from bottom up to find the surface
            for (int y = bounds.yMax; y >= bounds.yMin; y--)
            {
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    if (groundTilemap.HasTile(new Vector3Int(x, y, 0)))
                    {
                        groundLevel = y;
                        found = true;
                        Debug.Log($"[WorldGenerator] Auto-detected ground level at Y={groundLevel}");
                        goto FoundLevel;
                    }
                }
            }
            FoundLevel:;
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
