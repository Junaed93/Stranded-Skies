using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; 
    public float smoothSpeed = 0.15f;
    public Vector3 offset = new Vector3(0, 2f, -10); 

    [Header("Dynamic Zoom")]
    public float normalSize = 4f;
    public float zoomSize = 3f;
    public float zoomSpeed = 2f;
    public float enemyDetectionRadius = 6f;
    public LayerMask enemyLayer;

    private Camera cam;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Force correct values (overrides stale Inspector serialization)
        normalSize = 4f;
        zoomSize = 3f;
        offset = new Vector3(0, 2f, -10);

        if (cam != null) cam.orthographicSize = normalSize;
        if (enemyLayer == 0) enemyLayer = LayerMask.GetMask("Enemy");
    }

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                // Snap camera to player immediately on first find
                transform.position = target.position + offset;
                transform.position = new Vector3(transform.position.x, transform.position.y, -10);
            }
            return;
        }

        // Calculate desired camera position
        Vector3 desiredPos = target.position + offset;
        desiredPos.z = -10;

        // Smoothly follow the player
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothSpeed);

        HandleZoom();
    }

    void HandleZoom()
    {
        if (cam == null || target == null) return;

        Collider2D enemy = Physics2D.OverlapCircle(target.position, enemyDetectionRadius, enemyLayer);
        
        float targetSize = (enemy != null) ? zoomSize : normalSize;
        
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
    }

    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, enemyDetectionRadius);
        }
    }
}