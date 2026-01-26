using UnityEngine;

/// <summary>
/// PlayerSpawner.cs
/// Spawns the local player in the multiplayer scene.
/// Does NOT handle remote player spawning - that will be handled by network messages.
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("The local player prefab to instantiate")]
    public GameObject localPlayerPrefab;

    [Header("Spawn Settings")]
    [Tooltip("The spawn point for the local player")]
    public Transform spawnPoint;

    [Header("Runtime References")]
    [Tooltip("Reference to the spawned local player (set at runtime)")]
    public GameObject localPlayerInstance;

    void Awake()
    {
        Debug.Log("[PlayerSpawner] Awake called");
    }

    void Start()
    {
        Debug.Log("[PlayerSpawner] Start called");
        SpawnLocalPlayer();
    }


    /// <summary>
    /// Spawns the local player at the designated spawn point.
    /// </summary>
    public void SpawnLocalPlayer()
    {
        if (localPlayerPrefab == null)
        {
            Debug.LogError("[PlayerSpawner] localPlayerPrefab is not assigned!");
            return;
        }

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        localPlayerInstance = Instantiate(localPlayerPrefab, spawnPosition, spawnRotation);
        localPlayerInstance.name = "Player_Local";

        Debug.Log($"[PlayerSpawner] Local player spawned at {spawnPosition}");

        // Make camera follow the player
        CameraFollow cam = Camera.main?.GetComponent<CameraFollow>();
        if (cam != null)
        {
            cam.target = localPlayerInstance.transform;
            Debug.Log("[PlayerSpawner] Camera target set to player");
        }
        else
        {
            Debug.LogWarning("[PlayerSpawner] Could not find CameraFollow on Main Camera!");
        }
        
        // FREEZE PHYSICS immediately to prevent falling into void while world loads
        Rigidbody2D rb = localPlayerInstance.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
             rb.simulated = false; // Stop everything
        }
        
        // Start searching for ground
        Invoke(nameof(SnapToGround), 0.1f);
    }

    void SnapToGround()
    {
        if (localPlayerInstance == null) return;
        
        // METHOD 1: Ask WorldGenerator for the Absolute Truth
        if (WorldGenerator.Instance != null)
        {
             Vector3 safeSpawn = WorldGenerator.Instance.GetSafeSpawnPosition();
             localPlayerInstance.transform.position = safeSpawn;
             
             // Unfreeze and finish
             UnfreezePlayer();
             Debug.Log($"[PlayerSpawner] Teleported to WorldGenerator Safe Spot: {safeSpawn}");
             return;
        }

        // METHOD 2: Raycast Fallback (if no generator)
        RaycastHit2D hitDown = Physics2D.Raycast(localPlayerInstance.transform.position, Vector2.down, Mathf.Infinity);
        RaycastHit2D hitUp = Physics2D.Raycast(localPlayerInstance.transform.position, Vector2.up, Mathf.Infinity);
        
        Vector3 targetPos = localPlayerInstance.transform.position;
        bool found = false;

        if (hitUp.collider != null) 
        {
            targetPos = hitUp.point + new Vector2(0, 1.5f);
            found = true;
        }
        else if (hitDown.collider != null)
        {
            targetPos = hitDown.point + new Vector2(0, 1.5f);
            found = true;
        }
        else
        {
            Debug.LogWarning($"[PlayerSpawner] SnapToGround FAILED (Attempt). Raycast hit NOTHING. Retrying...");
            Invoke(nameof(SnapToGround), 0.2f);
            return;
        }

        if (found)
        {
             localPlayerInstance.transform.position = targetPos;
             UnfreezePlayer();
             Debug.Log($"[PlayerSpawner] Player SNAPPED and UNFROZEN at {targetPos}");
        }
    }

    void UnfreezePlayer()
    {
         if (localPlayerInstance == null) return;
         Rigidbody2D rb = localPlayerInstance.GetComponent<Rigidbody2D>();
         if (rb != null) 
         {
             rb.linearVelocity = Vector2.zero;
             rb.simulated = true; 
         }
    }


    // =====================================================
    // STUB: Remote Player Spawning
    // These will be called when the server notifies us of other players.
    // =====================================================

    /// <summary>
    /// Spawns a remote player instance. Called when server notifies of a new player.
    /// </summary>
    /// <param name="playerId">Unique ID of the remote player</param>
    /// <param name="position">Initial position of the remote player</param>
    /// <param name="remotePlayerPrefab">Prefab to use for remote players</param>
    /// <param name="remotePlayersParent">Parent transform to organize remote players</param>
    public void SpawnRemotePlayer(string playerId, Vector2 position, GameObject remotePlayerPrefab, Transform remotePlayersParent)
    {
        // TODO: Implement this when networking is added
        // GameObject remote = Instantiate(remotePlayerPrefab, position, Quaternion.identity, remotePlayersParent);
        // remote.name = "Player_Remote_" + playerId;
        // RemotePlayerController controller = remote.GetComponent<RemotePlayerController>();
        // controller.playerId = playerId;
        Debug.Log($"[PlayerSpawner] SpawnRemotePlayer({playerId}, {position}) - Stub called");
    }
}
