using UnityEngine;
using TMPro;

public class PlayerNameTag : MonoBehaviour
{
    [Header("Name Settings")]
    [Tooltip("The player name to display. If empty, uses 'Player' as default.")]
    public string playerName = "Player";

    [Header("Appearance")]
    [Tooltip("Vertical offset above the sprite")]
    public float yOffset = 1.5f;

    [Tooltip("Font size for the name")]
    public float fontSize = 3f;

    [Tooltip("Name text color")]
    public Color nameColor = Color.white;

    [Tooltip("Optional outline color")]
    public Color outlineColor = Color.black;

    [Tooltip("Outline thickness (0 to disable)")]
    public float outlineThickness = 0.25f;

    private TextMeshPro nameText;

    private bool nameResolved = false;

    void Start()
    {
        // Force reset - overrides any stale Inspector value
        playerName = "Player";

        CreateNameTag();
        TryResolveName();
    }

    void Update()
    {
        if (!nameResolved)
        {
            TryResolveName();
        }
    }

    void TryResolveName()
    {
        if (ScoreReporter.Instance != null && !string.IsNullOrEmpty(ScoreReporter.Instance.playerName)
            && ScoreReporter.Instance.playerName != "Player")
        {
            playerName = ScoreReporter.Instance.playerName;
            nameResolved = true;
            UpdateName(playerName);
        }
        else
        {
            // Show default name until resolved
            UpdateName(playerName);
        }
    }

    void CreateNameTag()
    {
        GameObject textObj = new GameObject("NameTag");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, yOffset, 0);
        textObj.transform.localScale = Vector3.one;

        nameText = textObj.AddComponent<TextMeshPro>();
        nameText.text = playerName;
        nameText.fontSize = fontSize;
        nameText.color = nameColor;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.sortingOrder = 100;

        // Set outline for readability
        if (outlineThickness > 0)
        {
            nameText.outlineWidth = outlineThickness;
            nameText.outlineColor = outlineColor;
        }

        // Make sure the text doesn't flip with the sprite
        nameText.rectTransform.sizeDelta = new Vector2(4f, 1f);
    }

    void LateUpdate()
    {
        if (nameText == null) return;

        // Keep the name tag always facing right (unflipped) regardless of sprite direction
        Vector3 parentScale = transform.localScale;
        float flipX = Mathf.Sign(parentScale.x);
        nameText.transform.localScale = new Vector3(flipX, 1f, 1f);

        // Keep position above sprite
        nameText.transform.localPosition = new Vector3(0, yOffset, 0);
    }

    public void UpdateName(string newName)
    {
        playerName = newName;
        if (nameText != null)
            nameText.text = playerName;
    }
}
