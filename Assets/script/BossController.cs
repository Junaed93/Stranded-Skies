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
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float stopDistance = 1.5f;

    [Header("Ranges")]
    public float abilityRange = 2.2f;
    public float attackRange = 1.6f;

    [Header("Attack")]
    public float attackCooldown = 2f;

    private float nextAttackTime;
    private bool isAttacking;
    private bool isDead;
    private bool abilityPlayed;

    void Start()
    {
        currentHealth = maxHealth;

        if (!player)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (!animator)
            animator = GetComponent<Animator>();

        if (!rb)
            rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 🔒 ABSOLUTE STOP
        if (isDead || player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        FacePlayer();

        // Ability trigger (once)
        if (!abilityPlayed && distance <= abilityRange)
        {
            animator.SetTrigger("Ability");
            abilityPlayed = true;
        }

        if (isAttacking || Time.time < nextAttackTime)
        {
            StopMoving();
            return;
        }

        if (distance <= attackRange)
            DoMeleeAttack();
        else
            MoveTowardPlayer(distance);
    }

    // ---------------- MOVEMENT ----------------

    void MoveTowardPlayer(float distance)
    {
        if (isDead) return;

        if (distance <= stopDistance)
        {
            StopMoving();
            return;
        }

        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * moveSpeed, 0f);
        animator.SetBool("Run", true);
    }

    void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("Run", false);
    }

    void FacePlayer()
    {
        if (isDead) return;

        float dir = Mathf.Sign(player.position.x - transform.position.x);
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) * dir,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    // ---------------- ATTACK ----------------

    void DoMeleeAttack()
    {
        if (isDead) return;

        StopMoving();

        int attackIndex = Random.Range(1, 4);

        if (attackIndex == 1)
            animator.SetTrigger("Attack");
        else if (attackIndex == 2)
            animator.SetTrigger("Attack 2");
        else
            animator.SetTrigger("Attack 3");

        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
    }

    // Animation Event
    public void EndAttack()
    {
        if (isDead) return;
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

        // 🧊 HARD FREEZE PHYSICS
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = false;

        // ❌ Disable ALL colliders
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        // ❌ Cancel all actions
        isAttacking = false;
        abilityPlayed = true;

        // 🎬 Lock animation to Death
        animator.SetBool("Run", false);
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Attack 2");
        animator.ResetTrigger("Attack 3");
        animator.ResetTrigger("Ability");
        animator.SetTrigger("Death");

        // 🧱 Destroy boss wall
        if (bossWall != null)
            bossWall.DestroyWall();

        // 🔒 Disable this script permanently
        this.enabled = false;
    }
}
