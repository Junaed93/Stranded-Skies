using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Stats")]
    public float speed = 3f;
    public float stopDistance = 1.5f;
    public float chaseRange = 50f;

    [Header("Attack Stats")]
    public float attackRange = 2.5f;
    public float attackRate = 1f;
    public int attackDamage = 10;

    [Header("Edge Detection")]
    public bool enableEdgeDetection = true;
    public float groundCheckDistance = 10f;
    public float edgeCheckOffset = 0.5f;
    public LayerMask groundLayer;

    [Header("References")]
    public Transform player;
    public Animator animator;
    public EnemyController enemyHealth;
    public Rigidbody2D rb;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip attackSFX;

    bool facingRight = true;
    bool isAttacking;
    float nextAttackTime;

    void Start()
    {
        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        enemyHealth = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody2D>();

        if (!audioSource)
            audioSource = GetComponent<AudioSource>();

        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        if (enemyHealth == null || enemyHealth.currentHealth <= 0) return;
        
        // Continuously search for player if not found or lost (handled by Spawner usually, but backup needed)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) 
            {
                player = p.transform;
            }
            return;
        }


        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < chaseRange)
            FacePlayer();

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("Run", false);
            return;
        }

        if (distance <= attackRange)
        {
            TryAttack();
        }
        else if (distance <= chaseRange && distance > stopDistance)
        {
            if (!enableEdgeDetection || IsGroundAhead())
                Move();
            else
                Stop();
        }
        else
        {
            Stop();
        }
    }

    void Move()
    {
        float dir = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
        animator.SetBool("Run", true);
    }

    void Stop()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("Run", false);
    }

    void TryAttack()
    {
        if (Time.time < nextAttackTime) return;

        FacePlayer();
        Stop();

        int atk = Random.Range(1, 4);
        if (atk == 1) animator.SetTrigger("Attack");
        else if (atk == 2) animator.SetTrigger("Attack 2");
        else animator.SetTrigger("Attack 3");

        PlaySound(attackSFX);

        isAttacking = true;
        nextAttackTime = Time.time + 1f / attackRate;

        Invoke(nameof(DealDamage), 0.5f);
        Invoke(nameof(ForceEndAttack), 1.2f);
    }

    public void DealDamage()
    {
        if (!player) 
        {
            Debug.LogWarning($"[EnemyAI] {gameObject.name} DealDamage FAILED: player is NULL");
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        
        float dist = Vector2.Distance(transform.position, player.position);

        if (GameSession.Instance.mode == GameMode.SinglePlayer)
        {
             if (player.TryGetComponent(out IDamageable damageable))
             {
                 damageable.TakeDamage(attackDamage);
             }
             Debug.Log($"[EnemyAI] Hit Player for {attackDamage}");
        }
        else
        {
             bool networkActive = false; 
             
             if (!networkActive)
             {
                  if (player.TryGetComponent(out IDamageable damageable))
                  {
                      damageable.TakeDamage(attackDamage);
                      Debug.Log($"[Multiplayer Demo] {gameObject.name} hit Player for {attackDamage}");
                  }
                  else
                  {
                      Debug.LogWarning($"[EnemyAI] Player found but missing IDamageable component!");
                  }
             }
             
             Debug.Log($"[Multiplayer] Enemy {gameObject.name} attack processed.");
        }
    }

    void ForceEndAttack()
    {
        isAttacking = false;
    }

    void FacePlayer()
    {
        if (transform.position.x < player.position.x && !facingRight)
            Flip();
        else if (transform.position.x > player.position.x && facingRight)
            Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(
            facingRight ? 1 : -1,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    bool IsGroundAhead()
    {
        int layerMask = groundLayer;
        if (layerMask == LayerMask.GetMask("Ground"))
             layerMask |= LayerMask.GetMask("Default");

        float dir = facingRight ? 1f : -1f;
        Vector2 start = new Vector2(
            transform.position.x + edgeCheckOffset * dir,
            transform.position.y
        );

        RaycastHit2D hit = Physics2D.Raycast(
            start,
            Vector2.down,
            groundCheckDistance,
            layerMask
        );

        Debug.DrawRay(start, Vector2.down * groundCheckDistance,
            hit ? Color.green : Color.red);

        return hit.collider != null;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip && audioSource)
            audioSource.PlayOneShot(clip);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}
