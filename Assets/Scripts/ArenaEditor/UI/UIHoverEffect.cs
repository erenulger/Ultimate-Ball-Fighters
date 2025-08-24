using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public float transitionSpeed = 5f;
    
    private Image imageComponent;
    private Color targetColor;
    
    void Start()
    {
        imageComponent = GetComponent<Image>();
        if (imageComponent != null)
        {
            normalColor = imageComponent.color;
            targetColor = normalColor;
        }
    }
    
    void Update()
    {
        if (imageComponent != null)
        {
            imageComponent.color = Color.Lerp(imageComponent.color, targetColor, transitionSpeed * Time.deltaTime);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetColor = hoverColor;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        targetColor = normalColor;
    }
}