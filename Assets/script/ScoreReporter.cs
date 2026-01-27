using UnityEngine;
using System;

public class ScoreReporter : MonoBehaviour
{
    public static ScoreReporter Instance { get; private set; }

    [Serializable]
    public class GameOverData
    {
        public int finalScore;
        public string gameMode;
        public string timestamp;
        public string playerId; 
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

        string token = GetTokenFromUrlOrArgs();
        if (!string.IsNullOrEmpty(token))
        {
            authToken = token;
            Debug.Log($"[ScoreReporter] Auto-Detected Auth Token: {authToken.Substring(0, Mathf.Min(5, authToken.Length))}...");
        }
    }

    private string GetTokenFromUrlOrArgs()
    {
        string url = Application.absoluteURL;
        if (!string.IsNullOrEmpty(url) && url.Contains("?"))
        {
            var query = System.Web.HttpUtility.ParseQueryString(new System.Uri(url).Query);
            if (!string.IsNullOrEmpty(query.Get("token"))) return query.Get("token");
            if (!string.IsNullOrEmpty(query.Get("auth"))) return query.Get("auth");
        }

        }

        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if ((args[i] == "--auth-token" || args[i] == "-t") && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }

        return "";
    }

    public string authToken = "";
    public string loginUrl = "http://localhost:3000/login";

    public void ReportGameOver()
    {
        if (string.IsNullOrEmpty(authToken))
        {
            Debug.Log($"<b>[AUTH REQUIRED]</b> User not logged in. Redirecting to: {loginUrl}");
            Application.OpenURL(loginUrl);
            return;
        }

        GameOverData data = new GameOverData();
        
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

        data.timestamp = DateTime.UtcNow.ToString("o");
        data.playerId = "LocalPlayer";

        string jsonPayload = JsonUtility.ToJson(data, true);

        StartCoroutine(PostScore(jsonPayload));
    }

    private System.Collections.IEnumerator PostScore(string json)
    {
        string url = "http://localhost:8080/api/scores";

        Debug.Log($"[ScoreReporter] Posting Score to {url}...");

        Debug.Log($"[ScoreReporter] Posting Score to {url}...");

        var request = new UnityEngine.Networking.UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        
        request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        request.SetRequestHeader("Authorization", "Bearer " + authToken);

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
