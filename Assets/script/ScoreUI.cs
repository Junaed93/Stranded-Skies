using TMPro; // Ensure you have TextMeshPro installed
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI scoreText;

    void OnEnable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
    }

    void OnDisable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
    }

    void Start()
    {
        if (ScoreManager.Instance != null)
            UpdateScoreDisplay(ScoreManager.Instance.GetScore());
    }

    private void UpdateScoreDisplay(int currentScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }
}
