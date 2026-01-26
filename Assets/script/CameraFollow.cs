using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; 
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 1f, -10); 

    [Header("Borders")]
    public bool enableLimits = true;
    public float minY = 4.5f; // [FIXED] Higher value keeps camera ABOVE the floor
    public float maxY = 20f;

    [Header("Dynamic Zoom")]
    public float normalSize = 7f;
    public float zoomSize = 5f;
    public float zoomSpeed = 2f;
    public float enemyDetectionRadius = 6f;
    public LayerMask enemyLayer;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (enemyLayer == 0) enemyLayer = LayerMask.GetMask("Enemy");
        Debug.Log("[CameraFollow] Started - waiting for player");
    }

    void LateUpdate()
    {
        // Auto-find player if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("[CameraFollow] Found player: " + player.name);
            }
            else
            {
                // Only log occasionally to avoid spam
                if (Time.frameCount % 60 == 0)
                    Debug.Log("[CameraFollow] Still searching for Player tag...");
            }
            return;
        }

        // Follow the target
        Vector3 desiredPosition = target.position + offset;
        
        if (enableLimits)
        {
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        desiredPosition.z = -10; 
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 2. Dynamic Zoom
        HandleZoom();
    }


    void HandleZoom()
    {
        if (cam == null) return;

        // Check if any enemy is nearby
        Collider2D enemy = Physics2D.OverlapCircle(target.position, enemyDetectionRadius, enemyLayer);
        
        float targetSize = (enemy != null) ? zoomSize : normalSize;
        
        // Smoothly change the camera size
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
    }

    // DEBUG: See the zoom detection range in Editor
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, enemyDetectionRadius);
        }
    }
}