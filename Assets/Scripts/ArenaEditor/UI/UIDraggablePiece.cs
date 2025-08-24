using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIDraggablePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag Settings")]
    public GameObject ClassPrefab; // Tile'a spawn olacak prefab
    public Canvas parentCanvas;
    
    private GameObject draggingObject;
    private Image classImage;
    private Vector3 originalPosition;
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    
    void Start()
    {
        // Class image'ini bul (child olarak)
        //classImage = transform.Find("Class")?.GetComponent<Image>();
        classImage = transform.GetChild(0).GetComponent<Image>();
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (classImage == null) return;
        
        // Dragging objesi oluştur
        CreateDraggingObject();
        
        // Orijinal Class image'ini gizle
        if (canvasGroup == null)
        {
            canvasGroup = classImage.gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0.5f;
        canvasGroup.blocksRaycasts = false;
    }
    
    void CreateDraggingObject()
    {
        // Canvas altında yeni bir GameObject oluştur
        draggingObject = new GameObject("DraggingClass");
        draggingObject.transform.SetParent(parentCanvas.transform);
        
        // Image component ekle
        Image dragImage = draggingObject.AddComponent<Image>();
        dragImage.sprite = classImage.sprite;
        dragImage.color = classImage.color;
        
        // RectTransform ayarları
        RectTransform rectTransform = draggingObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = classImage.rectTransform.sizeDelta;
        
        // En üstte göster
        draggingObject.transform.SetAsLastSibling();
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (draggingObject != null)
        {
            // Mouse pozisyonunu takip et
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                eventData.position,
                parentCanvas.worldCamera,
                out position
            );
            
            draggingObject.transform.localPosition = position;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
        {
            // Get the mouse position in screen coordinates
            Vector3 mousePosition = Input.mousePosition;
            
            // Convert screen position to world position using Camera.main
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            
            // Set the Z position to 0 for 2D space
            worldPosition.z = 0f;
            
            // Instantiate the object at the calculated world position
            if (ClassPrefab != null)
            {
                GameObject instantiatedObject = Instantiate(ClassPrefab, worldPosition, Quaternion.identity);
                
                // Reset the local position to ensure it snaps to the grid correctly
                // This prevents any local position offsets from the prefab from affecting placement
                instantiatedObject.transform.localPosition = Vector3.zero;
                
                // Then set the world position again to ensure proper placement
                instantiatedObject.transform.position = worldPosition;
            }
            else
            {
                Debug.LogWarning("No object assigned to instantiate! Please assign a prefab in the inspector.");
            }
        
        // Dragging object'i temizle
        if (draggingObject != null)
        {
            Destroy(draggingObject);
        }
    }
    
    GameObject GetHitObject(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<UITileCell>() != null)
            {
                return result.gameObject;
            }
        }
        
        return null;
    }
}