using UnityEngine;
using System.Collections.Generic;

public class BoardManager2D : MonoBehaviour
{
    public int Columns { get; private set; }
    public int Rows { get; private set; }
    public float TileSize { get; private set; }

    // Index -> TileCell
    TileCell2D[,] grid;
    // TileCell -> piece
    Dictionary<TileCell2D, DraggablePiece2D> occupants = new();

    public void Init(int columns, int rows, float tileSize)
    {
        Columns = columns;
        Rows = rows;
        TileSize = tileSize;
        grid = new TileCell2D[columns, rows];
        occupants.Clear();
    }

    public void RegisterCell(TileCell2D cell)
    {
        if (cell.Index.x < 0 || cell.Index.x >= Columns) return;
        if (cell.Index.y < 0 || cell.Index.y >= Rows) return;
        grid[cell.Index.x, cell.Index.y] = cell;
    }

    public TileCell2D CellAt(Vector2Int index)
    {
        if (index.x < 0 || index.x >= Columns) return null;
        if (index.y < 0 || index.y >= Rows) return null;
        return grid[index.x, index.y];
    }

    public bool IsOccupied(TileCell2D cell) => occupants.ContainsKey(cell);

    public bool TryPlace(DraggablePiece2D piece, TileCell2D targetCell, bool allowReplace = false)
    {
        if (targetCell == null) return false;

        if (IsOccupied(targetCell) && !allowReplace) return false;

        // Clear previous
        if (piece.CurrentCell != null && occupants.TryGetValue(piece.CurrentCell, out var occ) && occ == piece)
            occupants.Remove(piece.CurrentCell);

        // If replacing is allowed, you can handle capture here:
        if (allowReplace && occupants.ContainsKey(targetCell))
        {
            var victim = occupants[targetCell];
            occupants.Remove(targetCell);
            if (victim) Destroy(victim.gameObject); // or send to graveyard
        }

        occupants[targetCell] = piece;
        piece.CurrentCell = targetCell;
        piece.transform.position = targetCell.Center; // snap
        return true;
    }

    // Convert a world position to nearest grid index (rounded), clamped to board bounds
    public Vector2Int WorldToNearestIndex(Vector3 worldPos)
    {
        // board-local
        Vector3 local = transform.InverseTransformPoint(worldPos);

        Vector2 boardSize = new Vector2(Columns * TileSize, Rows * TileSize);
        Vector2 origin = -boardSize * 0.5f + new Vector2(TileSize * 0.5f, TileSize * 0.5f);

        int ix = Mathf.RoundToInt((local.x - origin.x) / TileSize);
        int iy = Mathf.RoundToInt((local.y - origin.y) / TileSize);

        ix = Mathf.Clamp(ix, 0, Columns - 1);
        iy = Mathf.Clamp(iy, 0, Rows - 1);
        return new Vector2Int(ix, iy);
    }
    public TileCell2D NearestCellFromWorld(Vector3 worldPos)
    {
        var idx = WorldToNearestIndex(worldPos);
        return CellAt(idx);
    }
    
    public TileCell2D CellUnderPoint(Vector2 worldPoint, LayerMask tileMask)
    {
        var hit = Physics2D.OverlapPoint(worldPoint, tileMask);
        return hit ? hit.GetComponent<TileCell2D>() : null;
    }
}
