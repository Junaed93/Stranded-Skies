using UnityEngine;

public class GolemBoss : MonoBehaviour, IDamageable
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
    public int maxHealth = 500;
    int currentHealth;
    bool isDead;

    [Header("Detection")]
    public float detectRange = 8f;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Attack")]
    public float attackRange = 2.5f;   // Kept slightly larger for Golem
    public float attackCooldown = 2.5f;
    public int attackDamage = 35;
    public float attackHitRange = 3f;  // Kept slightly larger for Golem

    [Header("Edge Detection")]
    public float groundCheckDistance = 2f;
    public float edgeCheckOffset = 1f;
    public LayerMask groundLayer;

    float lastAttackTime;
    bool isAttacking;
    bool hasDealtDamage;
    float moveDir;
    bool phaseScoreGiven = false;

    void Start()
    {
        // In Multiplayer: All bosses have 500 HP and no boss walls
        if (GameSession.Instance != null && GameSession.Instance.mode == GameMode.Multiplayer)
        {
            maxHealth = 500;
            bossWall = null; // No boss walls in multiplayer
        }

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
        if (isDead) return;
        
        // Continuously search for player if not found
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
            return;
        }


        float distance = Vector2.Distance(transform.position, player.position);

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

        FacePlayer();

        if (isAttacking)
        {
            HandleAttackDamage();
            StopMoving();
            return;
        }

        if (!IsPlayerInFront())
        {
            StopMoving();
            return;
        }

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

    // ---------------- ATTACK ----------------

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

        Invoke(nameof(EndAttack), 1.5f); // Matches Golem's longer animation
    }

    void HandleAttackDamage()
    {
        if (hasDealtDamage) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackHitRange)
        {
            if (GameSession.Instance.mode == GameMode.SinglePlayer)
            {
                PlayerCombat pc = player.GetComponent<PlayerCombat>();
                if (pc != null)
                {
                    pc.TakeDamage(attackDamage);
                    hasDealtDamage = true;
                    Debug.Log("Golem hit player!");
                }
            }
            else
            {
                // Multiplayer: Request damage from server
                // PlayerNetworkSender.RequestDamage(player.gameObject, attackDamage);
                hasDealtDamage = true;
                Debug.Log("Golem attack intent sent in Multiplayer");
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

        // [NEW] Phase 1 score at 50% HP (Golem: 100 pts)
        if (!phaseScoreGiven && currentHealth <= maxHealth / 2)
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(100);
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

        // [NEW] Final kill score (Golem: 400 pts)
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(400);

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
