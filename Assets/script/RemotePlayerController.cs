using UnityEngine;

public class RemotePlayerController : MonoBehaviour
{
    public string playerId;
    
    private Vector3 targetPosition;
    private Animator anim;
    private SpriteRenderer sr;
    
    private float smoothTime = 0.1f;
    private Vector3 velocity;

    void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;
        
        targetPosition = transform.position;
    }

    public void UpdateState(float x, float y, float velX, bool isGrounded)
    {
        targetPosition = new Vector3(x, y, 0);

        if (anim)
        {
            anim.SetFloat("Speed", Mathf.Abs(velX));
            anim.SetBool("Grounded", isGrounded);
        }

        if (velX > 0.1f) Flip(false);
        else if (velX < -0.1f) Flip(true);
    }

    public void TriggerAnim(string triggerName)
    {
        if (anim) anim.SetTrigger(triggerName);
    }

    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    void Flip(bool facingLeft)
    {
        if (sr) sr.flipX = facingLeft;
    }
}
