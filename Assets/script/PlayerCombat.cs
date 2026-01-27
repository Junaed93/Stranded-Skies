using UnityEngine;

public class PlayerCombat : MonoBehaviour, IDamageable
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

    [Header("Survival Settings")]
    public int maxRespawns = 3;
    public float minHeight = -20f;
    private int currentRespawns;

    [Header("Heal/Regen Settings")]
    public float regenRate = 5f;
    public float regenDelay = 3f;
    private float lastDamageTime;
    private float regenAccumulator;

    [Header("Combo Settings")]
    public float comboResetTime = 0.9f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip swordSwingSFX;
    public AudioClip swordHitSFX;
    public AudioClip blockSFX;
    public AudioClip hurtSFX;
    public AudioClip deathSFX;

    [Header("Components")]
    public Animator animator;

    private SpriteRenderer spriteRenderer;

    private int comboIndex = 0;
    private float lastAttackTime;
    private bool isAttacking = false;

    private bool facingRight = true;

    private bool isBlocking = false;

    private Vector3 attackPointStartLocalPos;
    private AutoCheckpoint autoCheckpoint;

    void Start()
    {
        currentHealth = maxHealth;
        currentRespawns = maxRespawns;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        attackPointStartLocalPos = attackPoint.localPosition;
        autoCheckpoint = GetComponent<AutoCheckpoint>();

        if (PlayerHealthUI.Instance != null)
            PlayerHealthUI.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    void Update()
    {
        if (isDead) return;

        if (transform.position.y < minHeight)
        {
            Die();
        }

        HandleRegeneration();

        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(20);
        }

        HandleFacingDirection();

        if (Input.GetMouseButtonDown(1))
        {
            isBlocking = true;
            animator.SetBool("IdleBlock", true);
            animator.SetTrigger("Block");
            PlaySound(blockSFX);
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            isBlocking = false;
            animator.SetBool("IdleBlock", false);
        }

        bool blockHeld = Input.GetMouseButton(1);
        isBlocking = blockHeld;

        if (Input.GetButtonDown("Fire1") && !isAttacking && !isBlocking)
        {
            StartCombo();
        }
    }

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

    void StartCombo()
    {
        comboIndex++;
        if (comboIndex > 3) comboIndex = 1;

        animator.SetTrigger("Attack" + comboIndex);
        lastAttackTime = Time.time;
        isAttacking = true;

        PlaySound(swordSwingSFX);
        
        Invoke(nameof(DealDamage), 0.3f); 
        Invoke(nameof(EndAttack), 0.8f);
    }

    public void DealDamage()
    {
        int mask = enemyLayers;
        if (mask <= 1) mask = ~0; 

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            mask
        );

        foreach (Collider2D enemy in hitEnemies)
        {
             if (enemy.gameObject == gameObject) continue;

             if (GameSession.Instance.mode == GameMode.SinglePlayer)
             {
                 if (enemy.TryGetComponent(out IDamageable damageable))
                 {
                     damageable.TakeDamage(attackDamage);
                     PlaySound(swordHitSFX);
                     Debug.Log($"[PlayerCombat] HIT {enemy.name} for {attackDamage} damage.");
                 }
             }
             else
             {
                 if (enemy.TryGetComponent(out IDamageable damageable))
                 {
                     damageable.TakeDamage(attackDamage);
                     PlaySound(swordHitSFX);
                     Debug.Log($"[Multiplayer Demo] HIT {enemy.name} for {attackDamage} damage.");
                 }
             }
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        
        isAttacking = false;
        
        if (!Input.GetMouseButton(1))
            isBlocking = false; 

        if (Time.time - lastAttackTime > comboResetTime)
            comboIndex = 0;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (isBlocking)
        {
            animator.SetTrigger("Block");
            return;
        }

        if (GameSession.Instance.mode == GameMode.SinglePlayer)
        {
            ApplyDamage(damage);
        }
        else
        {
            ApplyDamage(damage);
            Debug.Log("[Multiplayer Demo] Player health applied locally.");
        }
    }

    public void ApplyDamage(int damage)
    {
        currentHealth -= damage;
        animator.SetTrigger("Hurt");
        PlaySound(hurtSFX);
        lastDamageTime = Time.time;

        if (PlayerHealthUI.Instance != null)
            PlayerHealthUI.Instance.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        lastDamageTime = 0;

        animator.SetBool("IsDead", true);
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

        if (currentRespawns > 0)
        {
            currentRespawns--;
            Debug.Log("Respawns remaining: " + currentRespawns);
            Invoke(nameof(Respawn), 1.5f);
        }
        else
        {
            Debug.Log("Game Over - No respawns left.");

            if (ScoreReporter.Instance != null)
            {
                ScoreReporter.Instance.ReportGameOver();
            }
            else
            {
                GameObject go = new GameObject("TemporaryScoreReporter");
                ScoreReporter tempReporter = go.AddComponent<ScoreReporter>();
                tempReporter.ReportGameOver();
            }

            GameOverPanel gameOver = FindFirstObjectByType<GameOverPanel>();
            if (gameOver != null)
            {
                gameOver.Show();
            }
        }
    }

    void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;

        if (PlayerHealthUI.Instance != null)
            PlayerHealthUI.Instance.UpdateHealth(currentHealth, maxHealth);

        animator.SetBool("IsDead", false);
        animator.Rebind();
        animator.Update(0f);
        animator.Play("Idle");

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Collider2D col = GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            
            Vector3 respawnPos = Vector3.zero;
            if (autoCheckpoint != null)
            {
                respawnPos = autoCheckpoint.GetLastSafePosition();
            }

            if (respawnPos.y < -5f)
            {
                respawnPos = new Vector3(10f, 2f, 0f);
                Debug.Log("[PlayerCombat] Checkpoint was UNSAFE (falling). Respawning at start area.");
            }

            rb.position = respawnPos;
            transform.position = respawnPos;
            rb.WakeUp();
        }

        if (col != null)
        {
            col.enabled = true;
        }

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = true;
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        if (PlayerHealthUI.Instance != null)
            PlayerHealthUI.Instance.UpdateHealth(currentHealth, maxHealth);

        Debug.Log("Healed! Current HP: " + currentHealth);
    }

    void HandleRegeneration()
    {
        if (isDead || currentHealth >= maxHealth) return;

        if (Time.time - lastDamageTime >= regenDelay)
        {
            regenAccumulator += regenRate * Time.deltaTime;

            if (regenAccumulator >= 1f)
            {
                int healAmount = Mathf.FloorToInt(regenAccumulator);
                currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
                regenAccumulator -= healAmount;

                if (PlayerHealthUI.Instance != null)
                    PlayerHealthUI.Instance.UpdateHealth(currentHealth, maxHealth);
            }
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
