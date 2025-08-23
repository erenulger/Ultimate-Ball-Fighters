using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkBar : MonoBehaviour

{
    [Header("Perk Bar Components")]
    public Canvas perkBarCanvas;
    public Image perkIcon;
    public TextMeshProUGUI perkText;

    [Header("Settings")]
    public bool followTarget = true;
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    private Transform target;
    private Camera mainCamera;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        mainCamera = Camera.main;
    }

    void Update()
    {
        if (target != null && followTarget)
        {
            Vector3 worldPosition = target.position + offset;
            transform.position = worldPosition;
        }
    }

    public void Initialize(Transform marbleTransform, Image perkIconImage, float currentPerkValue)
    {
        // Set the target for position following
        target = marbleTransform;
        
        // Set the perk icon if provided
        if (perkIconImage != null && perkIcon != null)
        {
            perkIcon.sprite = perkIconImage.sprite;
            perkIcon.color = perkIconImage.color;
        }
        
        UpdatePerkBar(currentPerkValue);
    }

    public void UpdatePerkBar(float currentPerkValue)
    {
        if (perkText != null)
        {
            perkText.text = currentPerkValue.ToString("F1"); // Format as whole number
        }
    }
}