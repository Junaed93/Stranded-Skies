using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Stats")]
    public float speed = 3f;
    public float stopDistance = 1.5f; // Distance to stop running
    public float chaseRange = 10f;    // Distance to start chasing

    [Header("Attack Stats")]
    public float attackRange = 2.5f;  // NEW: Must be larger than stopDistance
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
        // 1. DEAD CHECK: Stop everything if health is 0
        if (enemyHealth != null && enemyHealth.currentHealth <= 0) 
        {
            this.enabled = false; // Disable AI script
            animator.SetBool("Run", false); // Ensure run anim stops
            return; 
        }

        if (player == null) return; 

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 2. CHASE LOGIC
        if (distanceToPlayer < chaseRange && distanceToPlayer > stopDistance)
        {
            ChasePlayer();
        }
        else 
        {
            // Stop running if close enough OR too far
            StopRunning();
            
            // 3. ATTACK LOGIC (Improved)
            // Check if we are within ATTACK range (2.5) instead of STOP distance (1.5)
            if (distanceToPlayer <= attackRange)
            {
                if (Time.time >= nextAttackTime)
                {
                    AttackLogic();
                    nextAttackTime = Time.time + 1f / attackRate;
                }
            }
        }
    }

    void ChasePlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        animator.SetBool("Run", true);

        if (transform.position.x < player.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void StopRunning()
    {
        animator.SetBool("Run", false);
    }

    void AttackLogic()
    {
        int randomAttack = Random.Range(1, 4); 

        if (randomAttack == 1) animator.SetTrigger("Attack");
        else if (randomAttack == 2) animator.SetTrigger("Attack 2");
        else if (randomAttack == 3) animator.SetTrigger("Attack 3");

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
        Gizmos.color = Color.blue; // Visualize Attack Range
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}