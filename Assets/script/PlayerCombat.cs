using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxHealth = 100;
    int currentHealth;
    bool isDead = false;

    [Header("Attack Settings")]
    public Transform attackPoint; 
    public float attackRange = 0.5f;
    public int attackDamage = 20;
    public LayerMask enemyLayers; 

    [Header("Components")]
    public Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        if (Input.GetButtonDown("Fire1"))
        {
            Attack();
        }
    }

    void Attack()
    {
        // MATCHES YOUR PARAMETER: "Attack1"
        animator.SetTrigger("Attack1");

        // Detect enemies
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Damage them
        foreach(Collider2D enemy in hitEnemies)
        {
            EnemyController enemyScript = enemy.GetComponent<EnemyController>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(attackDamage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // MATCHES YOUR PARAMETER: "Hurt"
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        
        // MATCHES YOUR PARAMETERS: "Death" (Trigger) and "IsDead" (Bool)
        animator.SetTrigger("Death");
        animator.SetBool("IsDead", true);

        // Disable movement if you have a movement script attached
        // GetComponent<PlayerMovement>().enabled = false; 
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}