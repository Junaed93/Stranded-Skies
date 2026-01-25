using UnityEngine;

public class AutoCheckpoint : MonoBehaviour
{
    [Header("Settings")]
    public float saveInterval = 0.5f;
    public LayerMask groundLayer = ~0; // Default to all layers

    private Vector3 lastSafePosition;
    private float saveTimer;
    private Rigidbody2D rb;
    private Collider2D col;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        lastSafePosition = transform.position;
    }

    void Update()
    {
        saveTimer -= Time.deltaTime;
        
        if (saveTimer <= 0)
        {
            saveTimer = saveInterval;
            
            if (IsSafe())
            {
                lastSafePosition = transform.position;
            }
        }
    }

    bool IsSafe()
    {
        // 1. Check if we are physically grounded
        bool isGrounded = IsGrounded();
        
        // 2. Check vertical velocity (ensure we aren't falling rapidly or jumping)
        // Using 0.1f threshold to allow for minor fluctuations
        bool isStable = rb != null && Mathf.Abs(rb.linearVelocity.y) < 0.1f;

        return isGrounded && isStable;
    }

    bool IsGrounded()
    {
        if (col == null) return true; // Fallback

        // Raycast from center down
        float extraHeight = 0.1f;
        RaycastHit2D hit = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.down, extraHeight, groundLayer);
        
        return hit.collider != null;
    }

    public Vector3 GetLastSafePosition()
    {
        return lastSafePosition;
    }

    // Visualization for debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lastSafePosition, 0.5f);
    }
}
