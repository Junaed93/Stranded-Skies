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
        
        // In SinglePlayer AND Multiplayer, we now spawn immediately.
        // MultiplayerBootstrap will handle the environment/seed, but the player is local.
        SpawnLocalPlayer();
    }

    public void SpawnPlayerAt(Vector3 position)
    {
        Debug.Log($"[PlayerSpawner] Spawn Command Received at {position}");
        
        if (spawnPoint == null)
        {
            GameObject sp = new GameObject("DynamicSpawnPoint");
            spawnPoint = sp.transform;
        }
        
        spawnPoint.position = position;
        SpawnLocalPlayer();
    }


    /// <summary>
    /// Spawns the local player at the designated spawn point.
    /// </summary>
    public void SpawnLocalPlayer()
    {
        if (localPlayerInstance != null)
        {
             Debug.Log("[PlayerSpawner] Local player already exists. Skipping spawn.");
             return;
        }

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

        // Register with WorldGenerator
        if (WorldGenerator.Instance != null)
        {
            WorldGenerator.Instance.RegisterPlayer(localPlayerInstance.transform);
            Debug.Log("[PlayerSpawner] Player registered with WorldGenerator");
        }
        else
        {
            Debug.LogError("[PlayerSpawner] WorldGenerator.Instance is NULL - could not register player!");
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
