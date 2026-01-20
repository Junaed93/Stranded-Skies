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

    [Header("Components")]
    public Animator animator;

    private int comboIndex = 0;
    private float lastAttackTime;
    private bool isAttacking = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            StartCombo();
        }
    }

    void StartCombo()
    {
        comboIndex++;
        if (comboIndex > 3)
            comboIndex = 1;

        animator.SetTrigger("Attack" + comboIndex);
        lastAttackTime = Time.time;
        isAttacking = true;
    }

    // 🔥 CALLED BY ANIMATION EVENT (HIT FRAME)
    public void DealDamage()
    {
        if (attackPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayers
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent(out EnemyController enemyController))
            {
                enemyController.TakeDamage(attackDamage);
            }
        }
    }

    // 🔚 CALLED BY ANIMATION EVENT (END FRAME)
    public void EndAttack()
    {
        isAttacking = false;

        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboIndex = 0;
        }
    }

    // ✅ ENEMY CALLS THIS
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");

        // Stop all other scripts (movement, jump, etc.)
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }

        // Freeze physics
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
