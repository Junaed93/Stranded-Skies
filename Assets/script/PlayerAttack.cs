using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Transform attackPoint;
    public float attackRange = 1f;
    public int damage = 20;
    public LayerMask enemyLayer;

    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    void Attack()
    {
        // 🔹 Play animation if exists
        if (anim) anim.SetTrigger("Attack1");

        Collider2D hit = Physics2D.OverlapCircle(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        if (hit)
        {
            Health enemy = hit.GetComponent<Health>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Enemy hit");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!attackPoint) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
