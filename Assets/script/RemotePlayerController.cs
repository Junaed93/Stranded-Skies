using UnityEngine;

/// <summary>
/// RemotePlayerController.cs
/// Controls remote player instances that are driven by network state updates.
/// - Rigidbody2D is Kinematic (no local physics simulation)
/// - No Update() logic or input handling
/// - State is applied via ApplyState() called by the network receiver
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class RemotePlayerController : MonoBehaviour
{
    [Header("Player Identity")]
    [Tooltip("Unique ID assigned by the server")]
    public string playerId;

    [Header("Components")]
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [Header("State")]
    [Tooltip("Last known position from the server")]
    private Vector2 lastPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();

        // Remote players must be kinematic - no local physics
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
    }

    /// <summary>
    /// Applies the state received from the server.
    /// Called by SocketReceiver when a state update is received.
    /// </summary>
    /// <param name="position">The new position of the remote player</param>
    public void ApplyState(Vector2 position)
    {
        // Calculate direction for flipping sprite
        float direction = position.x - lastPosition.x;

        // Move to the new position
        rb.MovePosition(position);

        // Flip sprite based on movement direction
        if (spriteRenderer != null && Mathf.Abs(direction) > 0.01f)
        {
            spriteRenderer.flipX = direction < 0;
        }

        // Update animation based on movement
        if (animator != null)
        {
            float speed = (position - lastPosition).magnitude / Time.deltaTime;
            animator.SetFloat("Speed", speed);
        }

        lastPosition = position;
    }

    /// <summary>
    /// Applies an attack state received from the server.
    /// Triggers attack animation on this remote player.
    /// </summary>
    /// <param name="attackIndex">The attack combo index (1, 2, or 3)</param>
    public void ApplyAttack(int attackIndex)
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack" + attackIndex);
        }
    }

    /// <summary>
    /// Applies a hurt state received from the server.
    /// </summary>
    public void ApplyHurt()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
    }

    /// <summary>
    /// Applies a death state received from the server.
    /// </summary>
    public void ApplyDeath()
    {
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            animator.SetTrigger("Death");
        }
    }
}
