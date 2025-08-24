using UnityEngine;
using UnityEngine.UI;

public class UITileCell : MonoBehaviour
{
    [Header("Tile Settings")]
    public bool allowPiecePlacement = true;
    public Transform pieceSpawnPoint; // Prefab'ın spawn olacağı nokta
    
    private GameObject currentPiece;
    private Image tileImage;
    
    void Start()
    {
        tileImage = GetComponent<Image>();
        
        // Eğer spawn point belirtilmemişse kendisini kullan
        if (pieceSpawnPoint == null)
            pieceSpawnPoint = transform;
    }
    
    public bool CanPlacePiece()
    {
        return allowPiecePlacement && currentPiece == null;
    }
    
    public void PlacePiece(GameObject piecePrefab)
    {
        if (!CanPlacePiece()) return;
        
        // Prefabı spawn et
        currentPiece = Instantiate(piecePrefab, pieceSpawnPoint);
        
        // Eğer UI elementiyse, RectTransform ayarları yap
        if (currentPiece.GetComponent<RectTransform>() != null)
        {
            RectTransform pieceRect = currentPiece.GetComponent<RectTransform>();
            pieceRect.localPosition = Vector3.zero;
            pieceRect.localScale = Vector3.one;
        }
        
        Debug.Log($"Piece placed on tile: {gameObject.name}");
    }
    
    public void RemovePiece()
    {
        if (currentPiece != null)
        {
            Destroy(currentPiece);
            currentPiece = null;
        }
    }
    
    public bool HasPiece()
    {
        return currentPiece != null;
    }
}