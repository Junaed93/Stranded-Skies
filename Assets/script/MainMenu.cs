using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlaySingle()
    {
        if (GameManager.Instance) GameManager.Instance.Mode = PlayMode.Single;
        if (GameSession.Instance) GameSession.Instance.mode = GameMode.SinglePlayer;
        SceneManager.LoadScene("SinglePlayer");
    }

    public void PlayMulti()
    {
        if (GameManager.Instance) GameManager.Instance.Mode = PlayMode.Multi;
        if (GameSession.Instance) GameSession.Instance.mode = GameMode.Multiplayer;
        SceneManager.LoadScene("Multiplayer");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
