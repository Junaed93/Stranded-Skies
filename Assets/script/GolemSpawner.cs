using UnityEngine;
using System.Collections.Generic;

public class GolemSpawner : MonoBehaviour
{
    public GameObject golemPrefab;

    public float minX;
    public float maxX;
    public float raycastHeight = 10f;

    public int maxSpawn = 3;
    public int maxAttempts = 30;
    public float minDistanceBetweenGolems = 3f; // ⭐ KEY FIX

    void Start()
    {
        SpawnOnce();
    }

    void SpawnOnce()
    {
        int spawnTarget = Random.Range(1, maxSpawn + 1);

        List<Vector2> usedPositions = new List<Vector2>();
        int attempts = 0;

        while (usedPositions.Count < spawnTarget && attempts < maxAttempts)
        {
            attempts++;

            float randomX = Random.Range(minX, maxX);
            Vector2 rayOrigin = new Vector2(randomX, transform.position.y + raycastHeight);

            RaycastHit2D hit = Physics2D.Raycast(
                rayOrigin,
                Vector2.down,
                raycastHeight * 2f
            );

            if (hit.collider == null)
                continue;

            // ❌ Block MovingPlatform
            if (hit.collider.gameObject.name.Contains("MovingPlatform"))
                continue;

            Vector2 spawnPos = hit.point;
            spawnPos.y += 0.1f;

            // ❌ Too close to another golem
            bool tooClose = false;
            foreach (Vector2 pos in usedPositions)
            {
                if (Vector2.Distance(pos, spawnPos) < minDistanceBetweenGolems)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
                continue;

            // ✅ Spawn
            Instantiate(golemPrefab, spawnPos, Quaternion.identity);
            usedPositions.Add(spawnPos);
        }

        this.enabled = false;
    }
}
