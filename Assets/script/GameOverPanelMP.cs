using UnityEngine;

public class GameOverPanelMP : MonoBehaviour
{
    [SerializeField] GameObject panel;

    void Awake()
    {
        panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);
        DisablePlayerInput();
    }

    void DisablePlayerInput()
    {
        PlayerCombat pc = FindObjectOfType<PlayerCombat>();
        if (pc != null)
            pc.enabled = false;
    }
}
