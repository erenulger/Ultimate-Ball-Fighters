using UnityEngine;
using UnityEngine.UI;

public class PerkBarManager : MonoBehaviour
{
    [Header("Health Bar References")]
    public PerkBar perkBarPrefab;

    [Header("Settings")]
    public Image perkIcon;
    public bool createPerkBarOnStart = true;
    public bool useWorldSpacePerkBar = true;

    private PerkBar currentPerkBar;
    private MeleeWeapon meleeWeapon;
    private Marble marble;

    void Awake()
    {
        meleeWeapon = GetComponentInChildren<MeleeWeapon>();
        marble = GetComponent<Marble>();
        if (meleeWeapon == null)
        {
            Debug.LogError("PerkBarManager: No MeleeWeapon component found on this GameObject!");
            return;
        }
        if (marble == null)
        {
            Debug.LogError("PerkBarManager: No Marble component found on this GameObject!");
            return;
        }

        // Subscribe to BOTH perk change events
        meleeWeapon.OnPerkChanged += OnPerkChanged;
        meleeWeapon.OnPerkChangedFloat += OnPerkChangedFloat;
    }

    void Start()
    {
        if (createPerkBarOnStart)
        {
            CreatePerkBar();
        }
    }

    public void OnDestroy()
    {
        // Unsubscribe from events
        if (meleeWeapon != null)
        {
            meleeWeapon.OnPerkChanged -= OnPerkChanged;
            meleeWeapon.OnPerkChangedFloat -= OnPerkChangedFloat;
        }

        // Clean up perk bar
        if (currentPerkBar != null)
        {
            Destroy(currentPerkBar.gameObject);
        }
    }

    public void CreatePerkBar()
    {
        if (perkBarPrefab == null)
        {
            Debug.LogError("PerkBarManager: perkBarPrefab is not assigned!");
            return;
        }
        if (currentPerkBar != null)
        {
            Destroy(currentPerkBar.gameObject);
        }

        // Instantiate the perk bar
        GameObject perkBarObj = Instantiate(perkBarPrefab.gameObject);
        currentPerkBar = perkBarObj.GetComponent<PerkBar>();

        currentPerkBar.Initialize(transform, perkIcon, meleeWeapon.GetCurrentPerk());

        // Configure canvas settings for world space
        if (useWorldSpacePerkBar && currentPerkBar.perkBarCanvas != null)
        {
            currentPerkBar.perkBarCanvas.renderMode = RenderMode.WorldSpace;
            currentPerkBar.perkBarCanvas.worldCamera = Camera.main;
        }
    }

    private void OnPerkChanged(int newPerk)
    {
        if (currentPerkBar != null)
        {
            currentPerkBar.UpdatePerkBar(newPerk);
        }
    }

    // Add this new method for float values
    private void OnPerkChangedFloat(float newPerk)
    {
        if (currentPerkBar != null)
        {
            currentPerkBar.UpdatePerkBar(newPerk);
        }
    }
}