using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public static BossHealthUI Instance;

    public Slider healthBar;

    private CanvasGroup canvasGroup;
    private MonoBehaviour activeBoss;
    private float closestDistance = float.MaxValue;
    private bool bossReportedThisFrame = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        Hide();
    }

    void LateUpdate()
    {
        // If no boss reported being in range this frame, hide the UI
        if (!bossReportedThisFrame)
        {
            activeBoss = null;
            Hide();
        }

        // Reset for next frame
        bossReportedThisFrame = false;
        closestDistance = float.MaxValue;
    }

    public void ReportProximity(MonoBehaviour boss, float distance, int currentHealth, int maxHealth)
    {
        bossReportedThisFrame = true;

        // Only show the CLOSEST boss if multiple are nearby
        if (activeBoss == null || distance < closestDistance)
        {
            activeBoss = boss;
            closestDistance = distance;
            
            Show();
            UpdateHealth(currentHealth, maxHealth);
        }
    }

    private void Show()
    {
        if (canvasGroup == null) return;
        
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    private void Hide()
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthBar != null)
        {
            healthBar.value = (float)current / max;
        }
    }
}
