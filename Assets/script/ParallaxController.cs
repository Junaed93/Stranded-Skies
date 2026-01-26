using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    private float startPosX, startPosY;
    private float length;

    public GameObject cam;
    [Range(0f, 1f)]
    public float parallaxEffect; // 0 = move with camera, 1 = no movement

    void Start()
    {
        startPosX = transform.position.x;
        startPosY = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void FixedUpdate()
    {
        // X movement
        float distX = cam.transform.position.x * parallaxEffect;
        float movementX = cam.transform.position.x * (1 - parallaxEffect);

        // Y movement
        float distY = cam.transform.position.y * parallaxEffect;

        transform.position = new Vector3(
            startPosX + distX,
            startPosY + distY,
            transform.position.z
        );

        // Infinite scrolling (X only)
        if (movementX > startPosX + length)
        {
            startPosX += length;
        }
        else if (movementX < startPosX - length)
        {
            startPosX -= length;
        }
    }
}
