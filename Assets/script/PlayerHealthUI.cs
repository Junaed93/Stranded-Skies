using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public static PlayerHealthUI Instance;

    public Slider healthBar;

    void Awake()
    {
        Instance = this;
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthBar != null)
        {
            healthBar.value = (float)current / max;
        }
    }
}
