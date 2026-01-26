using UnityEngine;

public enum GameMode
{
    SinglePlayer,
    Multiplayer
}

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public GameMode mode = GameMode.SinglePlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
