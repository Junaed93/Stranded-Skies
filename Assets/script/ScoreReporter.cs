using UnityEngine;
using System;

/// <summary>
/// ScoreReporter.cs
/// Handles data preparation and logging for Game Over events.
/// Prepares data for future backend POST integration.
/// </summary>
public class ScoreReporter : MonoBehaviour
{
    public static ScoreReporter Instance { get; private set; }

    [Serializable]
    public class GameOverData
    {
        public int finalScore;
        public string gameMode;
        public string timestamp;
        public string playerId; // Placeholder for future auth
    }

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

    public string authToken = ""; // Set by Launcher or Login Scene
    public string loginUrl = "http://localhost:3000/login";

    /// <summary>
    /// Prepares and logs Game Over data.
    /// Triggered when the local player runs out of respawns.
    /// </summary>
    public void ReportGameOver()
    {
        // [AUTH CHECK] Redirect to Web Launcher if not logged in
        if (string.IsNullOrEmpty(authToken))
        {
            Debug.Log($"<b>[AUTH REQUIRED]</b> User not logged in. Redirecting to: {loginUrl}");
            Application.OpenURL(loginUrl);
            return;
        }

        GameOverData data = new GameOverData();
        
        // Gather Data
        if (ScoreManager.Instance != null)
        {
            data.finalScore = ScoreManager.Instance.GetScore();
        }
        else
        {
            data.finalScore = 0;
            Debug.LogWarning("[ScoreReporter] ScoreManager not found, defaulting score to 0.");
        }

        if (GameSession.Instance != null)
        {
            data.gameMode = GameSession.Instance.mode.ToString();
        }
        else
        {
            data.gameMode = "Unknown";
        }

        data.timestamp = DateTime.UtcNow.ToString("o"); // ISO 8601
        data.playerId = "LocalPlayer"; // Stub

        // Send Data to Backend
        StartCoroutine(PostScore(jsonPayload));
    }

    /// <summary>
    /// Sends the Game Over data to the backend API.
    /// </summary>
    private System.Collections.IEnumerator PostScore(string json)
    {
        string url = "http://localhost:3000/api/scores"; // [CONFIG] Backend URL

        Debug.Log($"[ScoreReporter] Posting Score to {url}...");

        // Create Request
        var request = new UnityEngine.Networking.UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        
        // Headers
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        // Send & Wait
        yield return request.SendWebRequest();

        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Debug.Log("<b>[BACKEND SUCCESS]</b> Score uploaded successfully!");
            Debug.Log("Response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError($"<b>[BACKEND FAILED]</b> Error uploading score: {request.error}");
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }
}
