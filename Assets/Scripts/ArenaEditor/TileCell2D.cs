using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TileCell2D : MonoBehaviour
{
    public Vector2Int Index { get; private set; }
    public Vector3 Center => transform.position;
    public float TileSize { get; private set; }

    SpriteRenderer sr;
    Color baseColor;
    bool doHighlight;
    Color hoverColor;

    public void Setup(Vector2Int index, SpriteRenderer spriteRenderer, Color color, float tileSize, bool highlight)
    {
        Index = index;
        sr = spriteRenderer;
        baseColor = color;
        TileSize = tileSize;
        doHighlight = highlight;
        hoverColor = Color.Lerp(baseColor, Color.white, 0.2f);
    }

    void OnMouseEnter()
    {
        if (!doHighlight || sr == null) return;
        sr.color = hoverColor;
    }

    void OnMouseExit()
    {
        if (!doHighlight || sr == null) return;
        sr.color = baseColor;
    }

    public void Flash(Color c, float time = 0.1f)
    {
        if (sr == null) return;
        sr.color = c;
        CancelInvoke(nameof(ResetColor));
        Invoke(nameof(ResetColor), time);
    }

    void ResetColor()
    {
        if (sr != null) sr.color = baseColor;
    }
}
