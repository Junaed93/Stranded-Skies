using UnityEngine;

public class SkullBoss : MonoBehaviour, IDamageable
{
    [Header("References")]
    public Transform player;
    public Animator animator;
    public Rigidbody2D rb;
    public BossWall bossWall;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] attackSounds;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    [Header("Health")]
    public int maxHealth = 200;
    int currentHealth;
    bool isDead;

    [Header("Detection")]
    public float detectRange = 8f;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Attack")]
    public float attackRange = 1.6f;
    public float attackCooldown = 2f;
    public int attackDamage = 25;
    public float attackHitRange = 1.8f;

    [Header("Edge Detection")]
    public float groundCheckDistance = 2f;
    public float edgeCheckOffset = 1f;
    public LayerMask groundLayer;

    float lastAttackTime;
    bool isAttacking;
    bool hasDealtDamage;
    float moveDir;
    bool phaseScoreGiven = false; // [NEW]

    void Start()
    {
        currentHealth = maxHealth;

        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        rb.freezeRotation = true;
        rb.gravityScale = 1f;

        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // ❌ Player too far → idle
        if (distance > detectRange)
        {
            StopMoving();
            return;
        }

        // [NEW] Report Proximity to Boss UI
        if (BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.ReportProximity(this, distance, currentHealth, maxHealth);
        }

        // ✅ ALWAYS face player when detected
        FacePlayer();

        // ❌ Do nothing if attacking
        if (isAttacking)
        {
            HandleAttackDamage();
            StopMoving();
            return;
        }

        // ❌ Player must be in front to move/attack
        if (!IsPlayerInFront())
        {
            StopMoving();
            return;
        }

        // ✅ Attack
        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            StartAttack();
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    void FixedUpdate()
    {
        if (isDead || isAttacking) return;

        rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
    }

    // ---------------- ATTACK (NO EVENTS) ----------------

    void StartAttack()
    {
        isAttacking = true;
        hasDealtDamage = false;
        lastAttackTime = Time.time;

        animator.SetBool("Run", false);
        animator.SetTrigger("Attack");

        // Play Random Attack Sound
        if (attackSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];
            audioSource.PlayOneShot(clip);
        }

        Invoke(nameof(EndAttack), 1.2f);
    }

    void HandleAttackDamage()
    {
        if (hasDealtDamage) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackHitRange)
        {
            PlayerCombat pc = player.GetComponent<PlayerCombat>();
            if (pc != null)
            {
                pc.TakeDamage(attackDamage);
                hasDealtDamage = true;
            }
        }
    }

    void EndAttack()
    {
        isAttacking = false;
    }

    // ---------------- MOVEMENT ----------------

    void MoveTowardsPlayer()
    {
        if (!IsGroundAhead())
        {
            StopMoving();
            return;
        }

        moveDir = Mathf.Sign(player.position.x - transform.position.x);
        animator.SetBool("Run", true);
    }

    void StopMoving()
    {
        moveDir = 0;
        animator.SetBool("Run", false);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void FacePlayer()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        if (dir == 0) return;

        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) * dir,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    bool IsPlayerInFront()
    {
        float facing = Mathf.Sign(transform.localScale.x);
        float toPlayer = Mathf.Sign(player.position.x - transform.position.x);
        return facing == toPlayer;
    }

    // ---------------- EDGE DETECTION ----------------

    bool IsGroundAhead()
    {
        float dir = Mathf.Sign(transform.localScale.x);

        Vector2 start = new Vector2(
            transform.position.x + edgeCheckOffset * dir,
            transform.position.y
        );

        RaycastHit2D hit = Physics2D.Raycast(
            start,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        Debug.DrawRay(start, Vector2.down * groundCheckDistance, hit ? Color.green : Color.red);
        return hit.collider != null;
    }

    // ---------------- DAMAGE ----------------

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        animator.SetTrigger("Hit");

        // Play Hurt Sound
        if (hurtSound != null && audioSource != null)
            audioSource.PlayOneShot(hurtSound);

        // [NEW] Phase 1 score at 50% HP
        if (!phaseScoreGiven && currentHealth <= maxHealth / 2)
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(20);
            phaseScoreGiven = true;
        }

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        StopMoving();
        rb.simulated = false;

        // [NEW] Final kill score
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(80);

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        animator.SetTrigger("Death");

        // Play Death Sound
        if (deathSound != null && audioSource != null)
            audioSource.PlayOneShot(deathSound);

        if (bossWall != null)
            bossWall.DestroyWall();

        Destroy(gameObject, 2f);
    }
}
