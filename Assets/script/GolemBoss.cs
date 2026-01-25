using UnityEngine;

public class GolemBoss : MonoBehaviour, IDamageable
{
    [Header("References")]
    public Transform player;
    public Animator animator;
    public Rigidbody2D rb;
    public BossWall bossWall;

    [Header("Health")]
    public int maxHealth = 500;   // 🔥 MAIN BOSS HEALTH
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

    [Header("Attack Timing (NO EVENTS)")]
    public float attackHitDelay = 0.6f;
    public float attackDuration = 1.2f;

    [Header("Edge Detection")]
    public float groundCheckDistance = 2f;
    public float edgeCheckOffset = 1f;
    public LayerMask groundLayer;

    float lastAttackTime;
    float attackTimer;
    bool isAttacking;
    bool hasDealtDamage;
    float moveDir;
    bool isBossFightActive = false;
    bool bossMusicPlaying = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        rb.freezeRotation = true;

        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        UpdateBossMusic(distance);

        if (distance > detectRange)
        {
            StopMoving();
            return;
        }

        FacePlayer();

        if (isAttacking)
        {
            HandleAttack();
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

    // ================= ATTACK =================

    void StartAttack()
    {
        isAttacking = true;
        hasDealtDamage = false;
        attackTimer = 0f;
        lastAttackTime = Time.time;

        animator.SetBool("Run", false);

        int atk = Random.Range(1, 4);
        animator.SetTrigger("Attack" + atk);
    }

    void HandleAttack()
    {
        attackTimer += Time.deltaTime;

        if (!hasDealtDamage && attackTimer >= attackHitDelay)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= attackHitRange)
            {
                PlayerCombat pc = player.GetComponent<PlayerCombat>();
                if (pc != null)
                    pc.TakeDamage(attackDamage);
            }

            hasDealtDamage = true;
        }

        if (attackTimer >= attackDuration)
        {
            isAttacking = false;
        }
    }

    // ================= MOVEMENT =================

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

        return hit.collider != null;
    }

    // ================= DAMAGE =================

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        animator.SetTrigger("Hit");

        if (currentHealth <= 0)
            Die();
    }

    // ================= MUSIC =================

    void UpdateBossMusic(float distance)
    {
        // OUTSIDE BOSS AREA → ABSOLUTE RESET
        if (distance > detectRange)
        {
            if (isBossFightActive)
                EndBossFight();
            return;
        }

        // ENTER BOSS AREA
        if (!isBossFightActive)
            StartBossFight();

        // INSIDE BOSS AREA → FRONT CHECK
        if (IsPlayerInFront())
            PlayBossMusic();
        else
            StopBossMusic();
    }

    void StartBossFight()
    {
        isBossFightActive = true;
    }

    void EndBossFight()
    {
        StopBossMusic();
        isBossFightActive = false;
    }

    void PlayBossMusic()
    {
        if (bossMusicPlaying) return;

        if (BackgroundMusic.Instance != null)
        {
            BackgroundMusic.Instance.PlayBossMusic();
            bossMusicPlaying = true;
        }
    }

    void StopBossMusic()
    {
        if (!bossMusicPlaying) return;

        if (BackgroundMusic.Instance != null)
        {
            BackgroundMusic.Instance.PlayNormalMusic();
            bossMusicPlaying = false;
        }
    }

    bool IsPlayerInFront()
    {
        float bossFacing = Mathf.Sign(transform.localScale.x);
        float toPlayer = Mathf.Sign(player.position.x - transform.position.x);
        return bossFacing == toPlayer;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        StopMoving();
        rb.simulated = false;

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        animator.SetTrigger("Death");

        if (bossWall != null)
            bossWall.DestroyWall();

        EndBossFight();
        Destroy(gameObject, 2f);
    }
}
