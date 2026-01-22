using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public Transform attackPoint;
    public float attackRange = 1f;
    public int damage = 10;
    public LayerMask playerLayer;

    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void DoAttack()
    {
        // 🔹 Play animation if exists
        if (anim) anim.SetTrigger("Attack");

        Collider2D hit = Physics2D.OverlapCircle(
            attackPoint.position,
            attackRange,
            playerLayer
        );

        if (hit)
        {
            // Use PlayerCombat for damage so blocking works
            PlayerCombat playerCombat = hit.GetComponent<PlayerCombat>();
            if (playerCombat != null)
            {
                playerCombat.TakeDamage(damage);
                Debug.Log("Player hit");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!attackPoint) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
