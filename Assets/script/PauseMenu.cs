using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseUI;
    bool paused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused) Resume();
            else Pause();
        }
    }

    void Pause()
    {
        pauseUI.SetActive(true);
        Time.timeScale = 0f;
        paused = true;
    }

    public void Resume()
    {
        pauseUI.SetActive(false);
        Time.timeScale = 1f;
        paused = false;
    }

    public void QuitToMain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }
}
