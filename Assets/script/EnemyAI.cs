using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Stats")]
    public float speed = 3f;
    public float stopDistance = 1.5f;
    public float chaseRange = 10f;

    [Header("Attack Stats")]
    public float attackRate = 1f;
    public int attackDamage = 10;
    float nextAttackTime = 0f;

    [Header("References")]
    public Transform player;
    public Animator animator;
    public EnemyController enemyHealth;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
            
        enemyHealth = GetComponent<EnemyController>();
    }

    void Update()
    {
        if (enemyHealth == null || player == null) return; 

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < chaseRange && distanceToPlayer > stopDistance)
        {
            ChasePlayer();
        }
        else if (distanceToPlayer <= stopDistance)
        {
            StopRunning();
            
            if (Time.time >= nextAttackTime)
            {
                AttackLogic(); // <--- Changed function name
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
        else
        {
            StopRunning();
        }
    }

    void ChasePlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        animator.SetBool("Run", true);

        // Face the player
        if (transform.position.x < player.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void StopRunning()
    {
        animator.SetBool("Run", false);
    }

    // --- NEW RANDOM ATTACK LOGIC ---
    void AttackLogic()
    {
        // 1. Pick a random number between 1 and 3
        int randomAttack = Random.Range(1, 4); // (1, 4) returns 1, 2, or 3

        // 2. Trigger the correct animation based on the dice roll
        if (randomAttack == 1)
        {
            animator.SetTrigger("Attack");
        }
        else if (randomAttack == 2)
        {
            animator.SetTrigger("Attack 2"); // Make sure this matches your Parameter name exactly!
        }
        else if (randomAttack == 3)
        {
            animator.SetTrigger("Attack 3"); // Make sure this matches your Parameter name exactly!
        }

        // 3. Deal Damage
        PlayerCombat playerScript = player.GetComponent<PlayerCombat>();
        if (playerScript != null)
        {
            playerScript.TakeDamage(attackDamage);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}