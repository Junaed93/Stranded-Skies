using UnityEngine;

public class MultiplayerBootstrap : MonoBehaviour
{
    [SerializeField] private int forcedSeed = 0;

    void Start()
    {
        Debug.Log("[MultiplayerBootstrap] Start");

        if (forcedSeed == 0)
            forcedSeed = System.DateTime.Now.Millisecond;

        if (WorldGenerator.Instance != null)
        {
            WorldGenerator.Instance.InitializeWorld(forcedSeed);
            Debug.Log("[MultiplayerBootstrap] World initialized");
            
            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.EnableSpawning(true);
            }
            else
            {
                Debug.LogWarning("[MultiplayerBootstrap] EnemySpawner.Instance NOT found. Enemies won't spawn.");
            }
        }
        else
        {
            Debug.LogError("[MultiplayerBootstrap] WorldGenerator.Instance is NULL");
        }
    }
}
