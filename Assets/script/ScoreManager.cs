using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [SerializeField] private int score = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("ScoreManager Initialized!");
    }

    public System.Action<int> OnScoreChanged; // [NEW] Event for UI update

    public void AddScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score); // [NEW] Notify UI
        Debug.Log($"Score Added: {amount} | Total Score: {score}");
    }

    public int GetScore()
    {
        return score;
    }
}
