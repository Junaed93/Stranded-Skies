using UnityEngine;

/// <summary>
/// SeedManager.cs
/// Generates and stores an integer seed for deterministic world generation.
/// Used ONLY in Multiplayer.scene for procedural content.
/// </summary>
public class SeedManager : MonoBehaviour
{
    public static SeedManager Instance { get; private set; }

    [Header("Seed Settings")]
    [Tooltip("The seed used for world generation. Set to 0 for random seed.")]
    public int seed = 0;

    [Tooltip("If true, generates a random seed on Start if seed is 0")]
    public bool autoGenerateSeed = true;

    /// <summary>
    /// The active Random.State initialized with the seed.
    /// </summary>
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

    /// <summary>
    /// Initializes the seed. If seed is 0 and autoGenerateSeed is true, generates a random seed.
    /// </summary>
    public void InitializeSeed()
    {
        if (seed == 0 && autoGenerateSeed)
        {
            seed = System.Environment.TickCount;
        }

        SeededRandom = new System.Random(seed);
        Debug.Log($"[SeedManager] World seed initialized: {seed}");
    }

    /// <summary>
    /// Sets a specific seed and reinitializes the random generator.
    /// </summary>
    /// <param name="newSeed">The new seed value</param>
    public void SetSeed(int newSeed)
    {
        seed = newSeed;
        SeededRandom = new System.Random(seed);
        Debug.Log($"[SeedManager] Seed changed to: {seed}");
    }

    /// <summary>
    /// Returns a deterministic random integer.
    /// </summary>
    public int NextInt()
    {
        return SeededRandom.Next();
    }

    /// <summary>
    /// Returns a deterministic random integer within a range.
    /// </summary>
    public int NextInt(int min, int max)
    {
        return SeededRandom.Next(min, max);
    }

    /// <summary>
    /// Returns a deterministic random float between 0 and 1.
    /// </summary>
    public float NextFloat()
    {
        return (float)SeededRandom.NextDouble();
    }

    /// <summary>
    /// Returns a deterministic random float within a range.
    /// </summary>
    public float NextFloat(float min, float max)
    {
        return min + (max - min) * NextFloat();
    }
}
