using UnityEngine;

public class FloatingPlatform : MonoBehaviour
{
    public float moveDistance = 2f;
    public float speed = 2f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * speed) * moveDistance;
        transform.position = new Vector3(
            startPos.x + x,
            startPos.y,
            startPos.z
        );
    }
}
