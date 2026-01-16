using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; 
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Limit Camera Movement")]
    public bool enableLimits = true;
    public float minY = -2f; // Don't show the void below
    public float maxY = 2f;  // Don't show the sky above

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Calculate where we WANT to be
        Vector3 desiredPosition = target.position + offset;
        
        // 2. Clamp the Y height (The Fix)
        if (enableLimits)
        {
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        // 3. Keep Z at -10 to see the 2D world
        desiredPosition.z = -10; 
        
        // 4. Move smoothly
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}