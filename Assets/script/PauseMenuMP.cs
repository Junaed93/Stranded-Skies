using UnityEngine;

public class PauseMenuMP : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;
    bool isPaused;

    void Start()
    {
        pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    void Pause()
    {
        isPaused = true;
        pausePanel.SetActive(true);

        DisablePlayerInput(true);
    }

    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);

        DisablePlayerInput(false);
    }

    void DisablePlayerInput(bool disable)
    {
        PlayerCombat pc = FindFirstObjectByType<PlayerCombat>();
        if (pc != null)
            pc.enabled = !disable;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
