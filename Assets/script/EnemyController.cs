using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    bool isDead;

    [Header("Components")]
    public Animator animator;
    public Rigidbody2D rb;
    public Collider2D myCollider;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hurtSFX;
    public AudioClip deathSFX;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 🔥 FORCE 2D AUDIO
    }

    void Start()
    {
        currentHealth = maxHealth;
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
            // Multiplayer: Wait for server
            // [DEMO FALLBACK] Apply locally
            ApplyDamage(damage);
            Debug.Log($"[Multiplayer Demo] Enemy {gameObject.name} took {damage} damage.");
        }
    }

    public void ApplyDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;

        if (currentHealth > 0)
        {
             animator.SetTrigger("Hit");
             PlaySound(hurtSFX);
        }
        else
        {
             Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        PlaySound(deathSFX);
        animator.SetTrigger("Death");

        // [NEW] Add score
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(50);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (myCollider != null)
            myCollider.enabled = false;

        // 🔥 Disable ALL scripts except this one
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour s in scripts)
        {
            if (s != this)
                s.enabled = false;
        }
    }

    // Called via Animation Event at the END of death animation
    public void OnDeathAnimationEnd()
    {
        Destroy(gameObject, 0.5f);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("❌ Missing audio clip on " + name);
            return;
        }

        audioSource.PlayOneShot(clip);
        Debug.Log("🔊 Playing sound: " + clip.name);
    }
}
