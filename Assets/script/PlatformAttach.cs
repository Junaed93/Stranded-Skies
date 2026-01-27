using UnityEngine;

public class PlatformAttach : MonoBehaviour
{
    private Transform originalParent;
    private Transform newParent;
    private bool detach;

    void Start()
    {
        originalParent = transform.parent;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            newParent = collision.transform;
            transform.SetParent(newParent);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            detach = true;
        }
    }

    void LateUpdate()
    {
        if (detach)
        {
            transform.SetParent(originalParent);
            detach = false;
        }
    }
}
