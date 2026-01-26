using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    void Awake()
    {
        // Always hidden on start
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        // Freeze game ONLY in singleplayer
        if (GameManager.Instance != null &&
            GameManager.Instance.Mode == PlayMode.Single)
        {
            Time.timeScale = 0f;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);

        // Restore time if it was frozen
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;
    }
}
