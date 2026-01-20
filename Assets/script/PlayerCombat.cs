using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 20;
    public LayerMask enemyLayers;

    [Header("Combo Settings")]
    public float comboResetTime = 0.9f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip swordSwingSFX;
    public AudioClip swordHitSFX;
    public AudioClip blockSFX;

    [Header("Components")]
    public Animator animator;

    private SpriteRenderer spriteRenderer;

    private int comboIndex = 0;
    private float lastAttackTime;
    private bool isAttacking = false;

    private bool facingRight = true;

    // 🔒 Block (TRIGGER – ONE HIT)
    private bool isBlocking = false;

    private Vector3 attackPointStartLocalPos;

    void Start()
    {
        currentHealth = maxHealth;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        attackPointStartLocalPos = attackPoint.localPosition;
    }

    void Update()
    {
        if (isDead) return;

        HandleFacingDirection();

        // 🛡️ BLOCK (RIGHT CLICK – TRIGGER)
        if (Input.GetMouseButtonDown(1))
        {
            isBlocking = true;
            animator.SetTrigger("Block");
            PlaySound(blockSFX);
        }

        // ⚔️ ATTACK
        if (Input.GetButtonDown("Fire1") && !isAttacking && !isBlocking)
        {
            StartCombo();
        }
    }

    // ---------------- FACING ----------------

    void HandleFacingDirection()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal > 0.1f && !facingRight)
            Flip();
        else if (horizontal < -0.1f && facingRight)
            Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;

        if (spriteRenderer != null)
            spriteRenderer.flipX = !spriteRenderer.flipX;

        Vector3 pos = attackPointStartLocalPos;
        pos.x *= facingRight ? 1 : -1;
        attackPoint.localPosition = pos;
    }

    // ---------------- ATTACK ----------------

    void StartCombo()
    {
        comboIndex++;
        if (comboIndex > 3) comboIndex = 1;

        animator.SetTrigger("Attack" + comboIndex);
        lastAttackTime = Time.time;
        isAttacking = true;

        PlaySound(swordSwingSFX);
    }

    // 🔥 ANIMATION EVENT (HIT FRAME)
    public void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayers
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(attackDamage);
                PlaySound(swordHitSFX);
            }
        }
    }

    // 🔚 ANIMATION EVENT (END FRAME)
    public void EndAttack()
    {
        isAttacking = false;
        isBlocking = false; // 🔓 reset block

        if (Time.time - lastAttackTime > comboResetTime)
            comboIndex = 0;
    }

    // ---------------- PLAYER DAMAGE + BLOCK ----------------

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // 🛡️ BLOCK CONSUMES ONE HIT
        if (isBlocking)
        {
            isBlocking = false;
            return; // ❌ no damage
        }

        currentHealth -= damage;
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip && audioSource)
            audioSource.PlayOneShot(clip);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
