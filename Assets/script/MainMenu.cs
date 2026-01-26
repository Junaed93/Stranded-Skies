using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlaySingle()
    {
        GameManager.Instance.Mode = PlayMode.Single;
        SceneManager.LoadScene("SinglePlayer");
    }

    public void PlayMulti()
    {
        GameManager.Instance.Mode = PlayMode.Multi;
        SceneManager.LoadScene("Multiplayer");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
