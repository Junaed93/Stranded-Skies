using UnityEngine;

public enum PlayMode
{
    Single,
    Multi
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayMode Mode;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
