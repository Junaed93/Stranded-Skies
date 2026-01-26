using UnityEngine;

// Enum removed - using definition from GameSession.cs

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }

    public GameMode currentMode = GameMode.SinglePlayer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool IsMultiplayer()
    {
        return currentMode == GameMode.Multiplayer;
    }
}
