using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    private bool isDead = false;

    [Header("Components")]
    public Animator animator;
    public Rigidbody2D rb;
    public Collider2D myCollider;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // ✅ CALLED BY PLAYER (via IDamageable)
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            animator.SetTrigger("Hit");
        }
        else
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");

        // Stop physics
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Disable collision
        if (myCollider != null)
            myCollider.enabled = false;

        // Disable all other scripts (AI, movement, etc.)
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }
    }

    // 🔥 ANIMATION EVENT (LAST FRAME OF DEATH)
    public void OnDeathAnimationEnd()
    {
        Destroy(gameObject, 0.5f);
    }
}
