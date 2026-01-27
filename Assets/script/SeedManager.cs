using UnityEngine;

public class SeedManager : MonoBehaviour
{
    public static SeedManager Instance { get; private set; }

    [Header("Seed Settings")]
    [Tooltip("The seed used for world generation. Set to 0 for random seed.")]
    public int seed = 0;

    [Tooltip("If true, generates a random seed on Start if seed is 0")]
    public bool autoGenerateSeed = true;

    public System.Random SeededRandom { get; private set; }

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
        InitializeSeed();
    }

    public void InitializeSeed()
    {
        if (seed == 0 && autoGenerateSeed)
        {
            seed = System.Environment.TickCount;
        }

        SeededRandom = new System.Random(seed);
        Debug.Log($"[SeedManager] World seed initialized: {seed}");
    }

    public void SetSeed(int newSeed)
    {
        seed = newSeed;
        SeededRandom = new System.Random(seed);
        Debug.Log($"[SeedManager] Seed changed to: {seed}");
    }

    public int NextInt()
    {
        return SeededRandom.Next();
    }

    public int NextInt(int min, int max)
    {
        return SeededRandom.Next(min, max);
    }

    public float NextFloat()
    {
        return (float)SeededRandom.NextDouble();
    }

    public float NextFloat(float min, float max)
    {
        return min + (max - min) * NextFloat();
    }
}
