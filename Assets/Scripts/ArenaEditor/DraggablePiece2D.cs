using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DraggablePiece2D : MonoBehaviour
{
    [Header("Drag")]
    public float liftOffset = 0.05f;       // tiny z-bump while dragging (for visuals)
    public float followLerp = 1f;          // 1 = instant; <1 = smooth follow
    public LayerMask tileLayerMask;        // set this to your tile layer

    [Header("Placement")]
    public bool allowReplace = false;      // if true, dropping on occupied cell replaces (capture)

    Camera cam;
    Vector3 grabOffset;
    bool dragging = false;
    float baseZ;
    int baseSortingOrder;
    SpriteRenderer sr;

    public TileCell2D CurrentCell { get; set; }
    BoardManager2D board;

    void Awake()
    {
        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();
        baseZ = transform.position.z;
        baseSortingOrder = sr ? sr.sortingOrder : 0;
        board = FindFirstObjectByType<BoardManager2D>();
    }

    void OnMouseDown()
    {
        if (cam == null) return;
        dragging = true;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = transform.position.z;
        grabOffset = transform.position - world;

        if (sr) sr.sortingOrder = baseSortingOrder + 100; // bring to front
        transform.position = new Vector3(transform.position.x, transform.position.y, baseZ - liftOffset);
    }

    void OnMouseDrag()
    {
        if (!dragging || cam == null) return;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = transform.position.z;
        Vector3 target = world + grabOffset;

        if (followLerp >= 1f) transform.position = target;
        else transform.position = Vector3.Lerp(transform.position, target, followLerp * Time.deltaTime);
    }

    void OnMouseUp()
    {
        if (!dragging) return;
        dragging = false;

        if (sr) sr.sortingOrder = baseSortingOrder;
        transform.position = new Vector3(transform.position.x, transform.position.y, baseZ);

        if (board == null || cam == null)
        {
            if (CurrentCell != null) transform.position = CurrentCell.Center;
            return;
        }

        // Cursor world pos
        Vector3 cursorWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        cursorWorld.z = 0f;

        // 1) Try exact tile under cursor (collider)
        var targetCell = board.CellUnderPoint(cursorWorld, tileLayerMask);
        if (board.TryPlace(this, targetCell, allowReplace)) return;

        // 2) Fallback: snap to nearest grid center by math (no colliders required)
        var nearestCell = board.NearestCellFromWorld(cursorWorld);
        if (board.TryPlace(this, nearestCell, allowReplace)) return;

        // 3) Still not possible → go back
        if (CurrentCell != null) transform.position = CurrentCell.Center;
    }
}
