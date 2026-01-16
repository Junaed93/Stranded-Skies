using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    private float startPos;
    private float length;

    public GameObject cam;
    [Range(0f, 1f)]
    public float parallaxEffect; // 0 = move with camera, 1 = no movement

    void Start()
    {
        startPos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void FixedUpdate()
    {
        float distance = cam.transform.position.x * parallaxEffect;
        float movement = cam.transform.position.x * (1 - parallaxEffect);

        transform.position = new Vector3(
            startPos + distance,
            transform.position.y,
            transform.position.z
        );

        // Infinite scrolling
        if (movement > startPos + length)
        {
            startPos += length;
        }
        else if (movement < startPos - length)
        {
            startPos -= length;
        }
    }
}
