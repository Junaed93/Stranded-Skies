using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] GameObject panel;

    void Awake()
    {
        panel.SetActive(false);
    }

    public void Show()
    {
        Debug.Log("🔥 GameOverPanel.Show()");
        panel.SetActive(true);
    }
}
