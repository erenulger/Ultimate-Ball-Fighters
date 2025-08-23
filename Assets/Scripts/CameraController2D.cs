using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController2D : MonoBehaviour
{
    [Header("Zoom")]
    [SerializeField] private float zoomMultiplier = 4f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 8f;
    [SerializeField] private float zoomSmoothTime = 0.15f;

    [Header("Pan - Drag (MMB)")]
    [SerializeField] private float dragPanSpeed = 1.0f;   // higher = faster drag

    [Header("Pan - Edge")]
    [SerializeField] private bool edgePanEnabled = true;
    [SerializeField] private int edgePixels = 16;
    [SerializeField] private float edgePanSpeed = 30f;     // world units/sec at mid zoom
    [SerializeField] private bool edgeSpeedScalesWithZoom = true;

    [Header("Pan - Keyboard (WASD/Arrows)")]
    [SerializeField] private float keyboardPanSpeed = 30f; // world units/sec at mid zoom
    [SerializeField] private bool keyboardSpeedScalesWithZoom = true;
    [SerializeField] private float fastMultiplier = 2f;    // hold Shift for this multiplier

    [Header("Smoothing")]
    [SerializeField] private float moveSmoothTime = 0.10f;

    private Camera cam;
    private float targetZoom;
    private float zoomVelocity;         // SmoothDamp helper for zoom
    private Vector3 targetPos;          // where we want to be
    private Vector3 moveVelocity;       // SmoothDamp helper for position
    private Vector3 lastMousePos;       // for drag
    private bool dragging;
    private float zLock;
    private float refZoom;              // midpoint for speed scaling

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam.orthographic) { cam.orthographic = true; }

        targetZoom = cam.orthographicSize;
        targetPos  = transform.position;
        zLock      = transform.position.z;
        lastMousePos = Input.mousePosition;

        refZoom = (minZoom + maxZoom) * 0.5f;
        if (refZoom <= 0f) refZoom = cam.orthographicSize;
    }

    void Update()
    {
        HandleZoomKeepingCursorAnchor();
        HandlePanDrag();
        //HandlePanEdge();
        HandlePanKeyboard();

        // Smoothly apply position
        Vector3 desired = new Vector3(targetPos.x, targetPos.y, zLock);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref moveVelocity, moveSmoothTime);
    }

    // --- FIX: Anchor the world point under the cursor across ALL zoom-smoothing frames ---
    private void HandleZoomKeepingCursorAnchor()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
        {
            targetZoom -= scroll * zoomMultiplier;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // If we are changing size (either due to new input or ongoing smoothing), keep the point under mouse fixed
        if (!Mathf.Approximately(cam.orthographicSize, targetZoom))
        {
            // world point under cursor BEFORE this frame's zoom step
            Vector3 mouseWorldBefore = cam.ScreenToWorldPoint(Input.mousePosition);

            // advance zoom one smooth step this frame
            float nextSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);
            cam.orthographicSize = nextSize;

            // world point under cursor AFTER this frame's zoom step
            Vector3 mouseWorldAfter = cam.ScreenToWorldPoint(Input.mousePosition);

            // shift target so the same world point stays under the cursor
            Vector3 delta = mouseWorldBefore - mouseWorldAfter;
            targetPos += delta;
        }
    }

    // --- Middle mouse drag panning ---
    private void HandlePanDrag()
    {
        if (Input.GetMouseButtonDown(2))
        {
            dragging = true;
            lastMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(2))
        {
            dragging = false;
        }
        if (!dragging) return;

        Vector3 mouseNow = Input.mousePosition;
        Vector3 deltaScreen = mouseNow - lastMousePos;
        lastMousePos = mouseNow;

        if (deltaScreen.sqrMagnitude > 0f)
        {
            // Convert screen pixels to world units (constant screen-space feel)
            float worldPerPixel = (cam.orthographicSize * 2f) / Screen.height;
            Vector3 deltaWorld = new Vector3(-deltaScreen.x, -deltaScreen.y, 0f) * worldPerPixel * dragPanSpeed;
            targetPos += deltaWorld;
        }
    }

    // --- Edge panning ---
    /*private void HandlePanEdge()
    {
        if (!edgePanEnabled) return;

        Vector2 m = Input.mousePosition;
        float x = 0f, y = 0f;

        if (m.x <= edgePixels) x = -1f;
        else if (m.x >= Screen.width - edgePixels) x = 1f;
        if (m.y <= edgePixels) y = -1f;
        else if (m.y >= Screen.height - edgePixels) y = 1f;

        if (x == 0f && y == 0f) return;

        Vector2 dir = new Vector2(x, y).normalized;
        float speed = edgePanSpeed;
        if (edgeSpeedScalesWithZoom) speed *= cam.orthographicSize / refZoom;

        targetPos += (Vector3)(dir * speed * Time.deltaTime);
    }*/

    // --- WASD / Arrow keys panning ---
    private void HandlePanKeyboard()
    {
        float ix = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float iy = Input.GetAxisRaw("Vertical");   // W/S or Up/Down
        if (Mathf.Approximately(ix, 0f) && Mathf.Approximately(iy, 0f)) return;

        Vector2 dir = new Vector2(ix, iy).normalized;

        float speed = keyboardPanSpeed;
        if (keyboardSpeedScalesWithZoom) speed *= cam.orthographicSize / refZoom;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) speed *= fastMultiplier;

        targetPos += (Vector3)(dir * speed * Time.deltaTime);
    }
}
