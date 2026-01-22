using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraFollowDynamicCombatZoom : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Tilemap worldTilemap;

    [Header("Follow")]
    public float smoothSpeed = 6f;

    [Header("Zoom")]
    public float normalZoom = 6f;
    public float combatZoom = 4.5f;
    public float zoomSpeed = 3f;
    public float combatDistance = 3f;

    [Header("Dynamic Bounds")]
    public float boundsUpdateInterval = 0.5f;

    private Camera cam;

    private Vector3 worldMin;
    private Vector3 worldMax;

    private float camHalfWidth;
    private float camHalfHeight;

    private float timer;

    void Start()
    {
        if (!player)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = normalZoom;

        UpdateWorldBounds();
        UpdateCameraSize();
    }

    void LateUpdate()
    {
        if (!player || !worldTilemap) return;

        // 🔁 Update tilemap bounds dynamically
        timer += Time.deltaTime;
        if (timer >= boundsUpdateInterval)
        {
            UpdateWorldBounds();
            timer = 0f;
        }

        // 🔍 COMBAT ZOOM
        bool inCombat = IsPlayerInCombat();
        float targetZoom = inCombat ? combatZoom : normalZoom;

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            zoomSpeed * Time.deltaTime
        );

        // 🔄 MUST update camera size AFTER zoom
        UpdateCameraSize();

        // 🎯 Desired position
        Vector3 desired = new Vector3(
            player.position.x,
            player.position.y,
            transform.position.z
        );

        // 🧱 HARD CLAMP (NO LEAK EVER)
        desired.x = ClampCamera(
            desired.x,
            worldMin.x,
            worldMax.x,
            camHalfWidth
        );

        desired.y = ClampCamera(
            desired.y,
            worldMin.y,
            worldMax.y,
            camHalfHeight
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desired,
            smoothSpeed * Time.deltaTime
        );
    }

    // ---------------- CLAMP CORE ----------------

    float ClampCamera(float target, float min, float max, float halfSize)
    {
        // If map is smaller than camera → lock center
        if ((max - min) < halfSize * 2f)
            return (min + max) * 0.5f;

        return Mathf.Clamp(target, min + halfSize, max - halfSize);
    }

    // ---------------- WORLD BOUNDS ----------------

    void UpdateWorldBounds()
    {
        // Convert LOCAL tilemap bounds → WORLD bounds
        Bounds local = worldTilemap.localBounds;

        worldMin = worldTilemap.transform.TransformPoint(local.min);
        worldMax = worldTilemap.transform.TransformPoint(local.max);
    }

    // ---------------- CAMERA SIZE ----------------

    void UpdateCameraSize()
    {
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;
    }

    // ---------------- COMBAT CHECK ----------------

    bool IsPlayerInCombat()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            player.position,
            combatDistance,
            LayerMask.GetMask("Enemy")
        );

        return enemies.Length > 0;
    }

    // Debug
    void OnDrawGizmosSelected()
    {
        if (!player) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, combatDistance);
    }
}
