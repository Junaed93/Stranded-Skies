using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GolemSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject golemPrefab;

    [Tooltip("Exact tag used by ground colliders")]
    public string groundTag = "Ground";

    [Header("Spawn Area (X only)")]
    public float minX;
    public float maxX;

    [Header("Spawn Logic")]
    public int minSpawn = 1;
    public int maxSpawn = 3;
    public float minDistanceBetweenGolems = 3f;
    public float verticalOffset = 0.1f;
    public float initialSpawnDelay = 1f;
    public float respawnDelay = 3f;

    List<GameObject> activeGolems = new List<GameObject>();
    List<Collider2D> groundColliders = new List<Collider2D>();

    void Start()
    {
        if (!golemPrefab)
        {
            Debug.LogError("❌ Golem prefab missing");
            return;
        }

        CacheGround();
        StartCoroutine(InitialSpawn());
    }

    void Update()
    {
        CheckAndRespawnDeadGolems();
    }

    void CacheGround()
    {
        groundColliders.Clear();

        GameObject[] grounds = GameObject.FindGameObjectsWithTag(groundTag);
        foreach (GameObject g in grounds)
        {
            Collider2D col = g.GetComponent<Collider2D>();
            if (col != null)
                groundColliders.Add(col);
        }

        if (groundColliders.Count == 0)
            Debug.LogError("No ground colliders found with tag: " + groundTag);
    }

    IEnumerator InitialSpawn()
    {
        yield return new WaitForSeconds(initialSpawnDelay);
        SpawnGolems();
    }

    void CheckAndRespawnDeadGolems()
    {
        for (int i = activeGolems.Count - 1; i >= 0; i--)
        {
            if (activeGolems[i] == null)
            {
                activeGolems.RemoveAt(i);
                StartCoroutine(RespawnAfterDelay());
            }
        }
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnSingleGolem();
    }

    void SpawnGolems()
    {
        int spawnTarget = Random.Range(minSpawn, maxSpawn + 1);
        
        for (int i = 0; i < spawnTarget; i++)
        {
            SpawnSingleGolem();
        }

        Debug.Log($"✅ Spawned {activeGolems.Count} golems");
    }

    void SpawnSingleGolem()
    {
        int attempts = 0;
        int maxAttempts = 50;

        while (attempts < maxAttempts)
        {
            attempts++;

            Collider2D ground = groundColliders[Random.Range(0, groundColliders.Count)];
            Bounds b = ground.bounds;

            float randomX = Random.Range(
                Mathf.Max(minX, b.min.x),
                Mathf.Min(maxX, b.max.x)
            );

            Vector2 spawnPos = new Vector2(
                randomX,
                b.max.y + verticalOffset
            );

            if (IsTooCloseToActiveGolems(spawnPos)) continue;

            GameObject newGolem = Instantiate(golemPrefab, spawnPos, Quaternion.identity);
            activeGolems.Add(newGolem);
            Debug.Log($"🪨 Golem spawned at {spawnPos}");
            return;
        }

        Debug.LogWarning("⚠️ Could not find valid spawn position after max attempts");
    }

    bool IsTooCloseToActiveGolems(Vector2 pos)
    {
        foreach (GameObject golem in activeGolems)
        {
            if (golem != null)
            {
                if (Vector2.Distance(golem.transform.position, pos) < minDistanceBetweenGolems)
                    return true;
            }
        }
        return false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(minX, transform.position.y), new Vector3(maxX, transform.position.y));
    }
#endif
}
