using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class BootLoader : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string getGameMode();
#endif

    void Start()
    {
        string mode = "single"; // default

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            mode = getGameMode();
        }
        catch
        {
            mode = "single";
        }
#endif

        if (mode == "multi")
        {
            GameManager.Instance.Mode = PlayMode.Multi;
            SceneManager.LoadScene("Multiplayer");
        }
        else
        {
            GameManager.Instance.Mode = PlayMode.Single;
            SceneManager.LoadScene("SinglePlayer");
        }
    }
}
