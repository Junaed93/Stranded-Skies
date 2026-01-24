using UnityEngine;

public class SkullBoss : MonoBehaviour, IDamageable
{
    [Header("References")]
    public Transform player;
    public Animator animator;
    public Rigidbody2D rb;
    public BossWall bossWall;

    [Header("Health")]
    public int maxHealth = 300;
    int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float stopDistance = 1.5f;

    [Header("Ranges")]
    public float attackRange = 1.6f;
    public float abilityRange = 2.5f;

    [Header("Attack")]
    public float attackCooldown = 2f;
    public int attackDamage = 25;
    public float attackHitRange = 1.8f;

    [Header("Edge Detection")]
    public float groundCheckDistance = 2f;
    public float edgeCheckOffset = 1f;
    public LayerMask groundLayer;

    float lastAttackTime;
    bool isAttacking;
    bool isDead;
    bool abilityPlayed;

    float moveDir;

    void Start()
    {
        currentHealth = maxHealth;

        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (!animator)
            animator = GetComponent<Animator>();

        if (!rb)
            rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 1f;
        rb.freezeRotation = true;

        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        if (isDead || player == null) return;

        float horizontalDistance = Mathf.Abs(transform.position.x - player.position.x);

        // 🔴 FRONT CHECK
        if (!IsPlayerInFront())
        {
            moveDir = 0;
            animator.SetBool("Run", false);
            return;
        }

        FacePlayer();

        // Ability plays once
        if (!abilityPlayed && horizontalDistance <= abilityRange)
        {
            animator.SetTrigger("Ability");
            abilityPlayed = true;
        }

        if (isAttacking)
        {
            moveDir = 0;
            return;
        }

        if (horizontalDistance <= attackRange)
        {
            TryAttack();
            moveDir = 0;
        }
        else
        {
            if (IsGroundAhead())
            {
                moveDir = Mathf.Sign(player.position.x - transform.position.x);
                animator.SetBool("Run", true);
            }
            else
            {
                moveDir = 0;
                animator.SetBool("Run", false);
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
    }

    // ---------------- FRONT CHECK ----------------

    bool IsPlayerInFront()
    {
        float facingDir = Mathf.Sign(transform.localScale.x);
        float playerDir = Mathf.Sign(player.position.x - transform.position.x);

        return facingDir == playerDir;
    }

    // ---------------- EDGE DETECTION ----------------

    bool IsGroundAhead()
    {
        float direction = Mathf.Sign(player.position.x - transform.position.x);

        Vector2 rayStart = new Vector2(
            transform.position.x + (edgeCheckOffset * direction),
            transform.position.y
        );

        RaycastHit2D hit = Physics2D.Raycast(
            rayStart,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        Debug.DrawRay(
            rayStart,
            Vector2.down * groundCheckDistance,
            hit ? Color.green : Color.red
        );

        return hit.collider != null;
    }

    // ---------------- MOVEMENT ----------------

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

    // ---------------- ATTACK ----------------

    void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        animator.SetBool("Run", false);

        int attackIndex = Random.Range(1, 4);

        if (attackIndex == 1)
            animator.SetTrigger("Attack");
        else if (attackIndex == 2)
            animator.SetTrigger("Attack 2");
        else
            animator.SetTrigger("Attack 3");

        isAttacking = true;
        lastAttackTime = Time.time;

        Invoke(nameof(ForceEndAttack), 1.2f);
    }

    public void DealDamage()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackHitRange)
        {
            PlayerCombat pc = player.GetComponent<PlayerCombat>();
            if (pc != null)
                pc.TakeDamage(attackDamage);
        }
    }

    void ForceEndAttack()
    {
        isAttacking = false;
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    // ---------------- DAMAGE ----------------

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        animator.SetTrigger("Hit");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        animator.SetBool("Run", false);
        animator.SetTrigger("Death");

        if (bossWall != null)
            bossWall.DestroyWall();

        enabled = false;
    }
}
