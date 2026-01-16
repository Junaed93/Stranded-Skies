using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    int currentHealth;

    [Header("Components")]
    public Animator animator;      
    public Rigidbody2D rb;         
    public Collider2D myCollider;  

    void Start()
    {
        currentHealth = maxHealth;
        rb.gravityScale = 1; 
        rb.freezeRotation = true; 
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        // MATCHES YOUR PARAMETER: "Hit"
        animator.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        animator.SetTrigger("Death");

        // ... your physics code ...
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        myCollider.enabled = false;

        // --- ADD THIS LINE BELOW ---
        // Disable the AI so it stops trying to attack/move while dead
        GetComponent<EnemyAI>().enabled = false; 
        
        this.enabled = false;
        Destroy(gameObject, 5f);
    }

}