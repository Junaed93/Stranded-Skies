using UnityEngine;

public class BossWall : MonoBehaviour
{
    public Collider2D wallCollider;
    public GameObject wallVisual;
    public GameObject destroyEffect;

    private bool destroyed;

    void Awake()
    {
        if (wallCollider == null)
            wallCollider = GetComponent<Collider2D>();

        if (wallVisual == null)
            wallVisual = gameObject;
    }

    public void DestroyWall()
    {
        if (destroyed) return;
        destroyed = true;

        if (wallCollider != null)
            wallCollider.enabled = false;

        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, Quaternion.identity);

        if (wallVisual != null)
            wallVisual.SetActive(false);

        Destroy(gameObject, 0.1f);
    }
}
