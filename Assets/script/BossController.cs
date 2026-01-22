using UnityEngine;

public class BossController : MonoBehaviour, IDamageable
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
    public int attackDamage = 25;  // Damage dealt to player
    public Transform attackPoint;  // Optional: for hitbox detection
    public float attackHitRange = 1.8f; // Range for attack hitbox

    [Header("Edge Detection")]
    public float groundCheckDistance = 2f;  // How far down to check for ground
    public float edgeCheckOffset = 1f;      // How far in front to check
    public LayerMask groundLayer;           // Assign "Ground" layer in Inspector

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

        // Auto-detect ground layer if not set
        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        if (isDead || player == null) return;

        float horizontalDistance = Mathf.Abs(transform.position.x - player.position.x);

        FacePlayer();

        // Ability ONCE
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
            // Check for edge before moving
            if (IsGroundAhead())
            {
                moveDir = Mathf.Sign(player.position.x - transform.position.x);
                animator.SetBool("Run", true);
            }
            else
            {
                // Stop at edge - don't fall!
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

    // ---------------- EDGE DETECTION ----------------

    bool IsGroundAhead()
    {
        // Determine facing direction based on player position
        float direction = Mathf.Sign(player.position.x - transform.position.x);

        // Raycast start position (in front of boss, at feet level)
        Vector2 rayStart = new Vector2(
            transform.position.x + (edgeCheckOffset * direction),
            transform.position.y
        );

        // Cast ray downward
        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, groundCheckDistance, groundLayer);

        // Debug visualization
        Debug.DrawRay(rayStart, Vector2.down * groundCheckDistance, hit ? Color.green : Color.red);

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

        // SAFETY RESET (in case animation event is missing)
        Invoke(nameof(ForceEndAttack), 1.2f);
    }

    // 🔥 ANIMATION EVENT - Call this from attack animation hit frame
    public void DealDamage()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackHitRange)
        {
            PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
            if (playerCombat != null)
            {
                playerCombat.TakeDamage(attackDamage);
                Debug.Log("Boss hit player for " + attackDamage + " damage!");
            }
        }
    }

    void ForceEndAttack()
    {
        isAttacking = false;
    }

    // Animation Event (preferred)
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

    // ---------------- GIZMOS ----------------

    void OnDrawGizmosSelected()
    {
        // Visualize edge detection ray
        float direction = 1f;
        if (player != null)
            direction = Mathf.Sign(player.position.x - transform.position.x);

        Vector3 rayStart = new Vector3(
            transform.position.x + (edgeCheckOffset * direction),
            transform.position.y,
            0
        );
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
