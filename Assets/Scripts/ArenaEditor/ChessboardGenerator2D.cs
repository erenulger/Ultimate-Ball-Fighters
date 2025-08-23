using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class ChessboardGenerator2D : MonoBehaviour
{
    [Header("Board Size (tiles)")]
    [Min(1)] public int columns = 8;
    [Min(1)] public int rows = 8;

    [Header("Tile Size (world units)")]
    [Min(0.01f)] public float tileSize = 1f;

    [Header("Appearance")]
    public Color tileColor = new Color(0.90f, 0.90f, 0.90f);
    public bool startWithLightTopLeft = true;

    [Header("Tile Setup")]
    public Sprite squareSprite;          // assign a square sprite (e.g., Unity's built-in Square)
    public string tileLayerName = "Default";
    public string tilesRootName = "_Tiles";
    public bool addTileHighlight = true; // adds TileCell2D & hover highlight

    Transform tilesRoot;
    BoardManager2D board;

    void OnEnable()
    {
        board = GetComponent<BoardManager2D>();
        if (!board) board = gameObject.AddComponent<BoardManager2D>();
        Generate();
    }

    public void Generate()
    {
        EnsureRoot();
        ClearChildren(tilesRoot);

        // Board centered on this transform
        Vector2 boardSize = new Vector2(columns * tileSize, rows * tileSize);
        Vector2 origin = -boardSize * 0.5f + new Vector2(tileSize * 0.5f, tileSize * 0.5f);

        board.Init(columns, rows, tileSize);

        int tileLayer = LayerMask.NameToLayer(tileLayerName);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                bool isLight = ((x + y) % 2 == 0) ? startWithLightTopLeft : !startWithLightTopLeft;
                Vector3 localPos = new Vector3(origin.x + x * tileSize, origin.y + y * tileSize, 0f);

                var go = new GameObject($"Tile_{x}_{y}");
                go.transform.SetParent(tilesRoot, false);
                go.transform.localPosition = localPos;
                if (tileLayer >= 0) go.layer = tileLayer;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = squareSprite;
                sr.color = isLight ? tileColor : tileColor;

                // Scale to exact tileSize
                if (squareSprite != null)
                {
                    Vector2 s = squareSprite.bounds.size;
                    go.transform.localScale = new Vector3(
                        tileSize / Mathf.Max(1e-6f, s.x),
                        tileSize / Mathf.Max(1e-6f, s.y),
                        1f);
                }
                else
                {
                    go.transform.localScale = Vector3.one * tileSize;
                }

                // Collider for raycast/drag-drop
                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = false; // physical if you want
                col.size = Vector2.one; // because we scaled transform to size

                // Tile meta
                var cell = go.AddComponent<TileCell2D>();
                cell.Setup(new Vector2Int(x, y), sr, isLight ? tileColor : tileColor, tileSize, addTileHighlight);

                board.RegisterCell(cell);
            }
        }
    }

    public void GenerateSmall()
    {
        rows = 4;
        columns = 4;
        Generate();
    }

    public void GenerateMedium()
    {
        rows = 8;
        columns = 8;
        Generate();
    }

    public void GenerateLarge()
    {
        rows = 16;
        columns = 16;
        Generate();
    }

    public void GenerateHuge()
    {
        rows = 32;
        columns = 32;
        Generate();
    }

    void EnsureRoot()
    {
        if (tilesRoot == null)
        {
            var t = transform.Find(tilesRootName);
            if (t) tilesRoot = t;
            else
            {
                var go = new GameObject(tilesRootName);
                tilesRoot = go.transform;
                tilesRoot.SetParent(transform, false);
            }
        }
    }

    void ClearChildren(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            var c = t.GetChild(i);
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(c.gameObject);
            else Destroy(c.gameObject);
#else
            Destroy(c.gameObject);
#endif
        }
    }

#if UNITY_EDITOR
bool _regenQueued;

void OnValidate()
{
    columns  = Mathf.Max(1, columns);
    rows     = Mathf.Max(1, rows);
    tileSize = Mathf.Max(0.01f, tileSize);

    if (!isActiveAndEnabled) return;

    if (Application.isPlaying)
    {
        // In Play Mode it's fine to rebuild immediately.
        Generate();
        return;
    }

    // In Edit Mode: defer to next editor tick (outside OnValidate stack)
    if (_regenQueued) return;
    _regenQueued = true;
    EditorApplication.delayCall += () =>
    {
        _regenQueued = false;
        if (this == null) return; // component deleted
        if (!isActiveAndEnabled) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        Generate();
    };
}
#endif
}
