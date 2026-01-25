using TMPro; // Ensure you have TextMeshPro installed
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI scoreText;

    void Start()
    {
        // Subscribe in Start to ensure ScoreManager.Instance is ready
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            UpdateScoreDisplay(ScoreManager.Instance.GetScore());
        }
        else
        {
            Debug.LogError("ScoreUI: ScoreManager NOT FOUND! Did you add the ScoreManager object to your scene?");
        }
    }

    void OnDestroy()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
    }

    private void UpdateScoreDisplay(int currentScore)
    {
        Debug.Log("ScoreUI: Updating text to " + currentScore);
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }
}
