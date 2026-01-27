using UnityEngine;

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

    public void SpawnRemotePlayer(string playerId, Vector2 position, GameObject remotePlayerPrefab, Transform remotePlayersParent)
    {
        Debug.Log($"[PlayerSpawner] SpawnRemotePlayer({playerId}, {position}) - Stub called");
    }
}
