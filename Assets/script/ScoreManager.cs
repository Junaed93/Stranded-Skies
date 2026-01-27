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

    public System.Action<int> OnScoreChanged;

    public void AddScore(int amount)
    {
        if (GameSession.Instance.mode == GameMode.SinglePlayer)
        {
            ApplyScore(amount);
        }
        else
        {
        else
        {
            ApplyScore(amount);
            Debug.Log($"[Multiplayer Demo] Score added: {amount}");
        }
    }

    public void ApplyScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
        Debug.Log($"Score Added: {amount} | Total Score: {score}");
    }

    public int GetScore()
    {
        return score;
    }
}
