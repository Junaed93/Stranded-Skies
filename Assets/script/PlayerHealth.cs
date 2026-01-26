using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public bool isDead = false;

    Rigidbody2D rb;
    Collider2D col;

    void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (GameSession.Instance.mode == GameMode.SinglePlayer)
        {
            ApplyDamage(damage);
        }
        else
        {
            // Multiplayer: Request damage from server. 
            // Demo Fallback: If no server, apply immediately.
            ApplyDamage(damage);
            Debug.Log($"[Multiplayer] Player took {damage} damage (Demo Fallback).");
        }
    }

    // 🌐 Entry point for multiplayer damage confirmation or singleplayer application
    public void ApplyDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        // Stop physics
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Disable collisions
        if (col)
            col.enabled = false;

        // Disable all scripts except Health
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour s in scripts)
        {
            if (s != this)
                s.enabled = false;
        }

        Debug.Log(gameObject.name + " died");
    }
}
