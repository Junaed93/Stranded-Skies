using UnityEngine;

public class HeroKnightController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float rollForce = 8f;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    bool grounded;
    bool rolling;
    int attackStep = 0;
    float attackTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        attackTimer += Time.deltaTime;

        float x = Input.GetAxisRaw("Horizontal");

        // MOVE
        if (!rolling)
            rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);

        // FLIP
        if (x != 0)
            sr.flipX = x < 0;

        // ANIM DATA
        anim.SetFloat("Speed", Mathf.Abs(x));
        anim.SetFloat("AirY", rb.linearVelocity.y);
        anim.SetBool("Grounded", grounded);

        // JUMP
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            grounded = false;
            anim.SetTrigger("Jump");
        }

        // ATTACK COMBO
        if (Input.GetMouseButtonDown(0) && attackTimer > 0.3f)
        {
            attackStep++;
            if (attackStep > 3) attackStep = 1;

            anim.SetTrigger("Attack" + attackStep);
            attackTimer = 0f;
        }

        if (attackTimer > 1f)
            attackStep = 0;

        // BLOCK
        if (Input.GetMouseButtonDown(1))
        {
            anim.SetBool("IdleBlock", true);
            anim.SetTrigger("Block");
        }
        if (Input.GetMouseButtonUp(1))
            anim.SetBool("IdleBlock", false);

        // ROLL
        if (Input.GetKeyDown(KeyCode.LeftShift) && grounded)
        {
            rolling = true;
            anim.SetTrigger("Roll");
            rb.linearVelocity = new Vector2((sr.flipX ? -1 : 1) * rollForce, rb.linearVelocity.y);
            Invoke(nameof(StopRoll), 0.4f);
        }

        // HURT
        if (Input.GetKeyDown(KeyCode.Q))
            anim.SetTrigger("Hurt");

        // DEATH
        if (Input.GetKeyDown(KeyCode.E))
        {
            anim.SetBool("IsDead", true);
            anim.SetTrigger("Death");
            enabled = false;
        }
    }

    void StopRoll()
    {
        rolling = false;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground") || col.gameObject.CompareTag("MovingPlatform"))
            grounded = true;
    } 
}