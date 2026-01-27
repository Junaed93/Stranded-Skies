using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    private float startPosX, startPosY;
    private float length;

    public GameObject cam;
    [Range(0f, 1f)]
    public float parallaxEffect;

    void Start()
    {
        startPosX = transform.position.x;
        startPosY = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void FixedUpdate()
    {
        float distX = cam.transform.position.x * parallaxEffect;
        float movementX = cam.transform.position.x * (1 - parallaxEffect);

        float distY = cam.transform.position.y * parallaxEffect;

        transform.position = new Vector3(
            startPosX + distX,
            startPosY + distY,
            transform.position.z
        );

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
