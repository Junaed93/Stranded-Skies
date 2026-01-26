using UnityEngine;

public class AutoCheckpoint : MonoBehaviour
{
    [Header("Settings")]
    public float saveInterval = 3.0f; // [USER REQUESTED] 3 second interval
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
        saveTimer = saveInterval;
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
        
        // 2. Check vertical velocity (ensure we aren't falling or jumping significantly)
        // Using 0.05f threshold for better precision
        bool isStable = rb != null && Mathf.Abs(rb.linearVelocity.y) < 0.05f;

        // 3. Ensure we aren't at a dangerously low Y (approaching fall threshold)
        bool aboveVoid = transform.position.y > -5f;

        return isGrounded && isStable && aboveVoid;
    }

    bool IsGrounded()
    {
        if (col == null) return false;

        // Raycast from bottom
        float extraHeight = 0.2f;
        RaycastHit2D hit = Physics2D.BoxCast(col.bounds.center, col.bounds.size * 0.9f, 0f, Vector2.down, extraHeight, groundLayer);
        
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
