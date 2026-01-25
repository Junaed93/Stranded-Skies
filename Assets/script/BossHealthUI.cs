using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public static BossHealthUI Instance;

    public Slider healthBar; // The Slider component

    private float hideTimer = 0f;
    private bool isVisible = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Ensure the bar starts hidden
        if (healthBar != null) 
            healthBar.gameObject.SetActive(false);
    }

    void Update()
    {
        // Auto-hide logic: If no report received for 0.1s, hide it.
        // This is more robust than LateUpdate frame-perfect logic for some setups.
        if (isVisible)
        {
            hideTimer += Time.deltaTime;
            if (hideTimer > 0.1f)
            {
                Hide();
            }
        }
    }

    public void ReportProximity(MonoBehaviour boss, float distance, int currentHealth, int maxHealth)
    {
        // Reset hide timer because a boss is claiming usage
        hideTimer = 0f;
        
        Show();
        UpdateHealth(currentHealth, maxHealth);
    }

    private void Show()
    {
        if (!isVisible)
        {
            isVisible = true;
            if (healthBar != null) healthBar.gameObject.SetActive(true);
        }
    }

    private void Hide()
    {
        if (isVisible)
        {
            isVisible = false;
            if (healthBar != null) healthBar.gameObject.SetActive(false);
        }
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthBar != null)
        {
            healthBar.value = (float)current / max;
        }
    }
}


