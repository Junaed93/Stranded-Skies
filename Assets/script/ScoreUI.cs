using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI scoreText;

    void Start()
    {
    void Start()
    {
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
