using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    [Header("Health Bar References")]
    public HealthBar healthBarPrefab;     // Drag your health bar prefab here
    
    [Header("Settings")]
    public bool createHealthBarOnStart = true;
    public bool useWorldSpaceHealthBar = true;  // World space vs Screen space
    
    private HealthBar currentHealthBar;
    private Marble marble;
    
    void Awake()
    {
        marble = GetComponent<Marble>();
        if (marble == null)
        {
            Debug.LogError("HealthBarManager: No Marble component found on this GameObject!");
            return;
        }
        
        // Subscribe to health change events
        marble.OnHealthChanged += OnHealthChanged;
    }
    
    void Start()
    {
        if (createHealthBarOnStart)
        {
            CreateHealthBar();
        }
    }
    
    public void OnDestroy()
    {
        // Unsubscribe from events
        if (marble != null)
        {
            marble.OnHealthChanged -= OnHealthChanged;
        }
        
        // Clean up health bar
        if (currentHealthBar != null)
        {
            Destroy(currentHealthBar.gameObject);
        }
    }
    
    public void CreateHealthBar()
    {
        if (healthBarPrefab == null)
        {
            Debug.LogError("HealthBarManager: healthBarPrefab is not assigned!");
            return;
        }
        
        if (currentHealthBar != null)
        {
            Destroy(currentHealthBar.gameObject);
        }
        
        // Instantiate the health bar
        GameObject healthBarObj = Instantiate(healthBarPrefab.gameObject);
        currentHealthBar = healthBarObj.GetComponent<HealthBar>();
  
        // Initialize the health bar with team information
        currentHealthBar.Initialize(transform, marble.GetMaxHealth(), marble.Team());
        
        // Configure canvas settings for world space
        if (useWorldSpaceHealthBar && currentHealthBar.healthBarCanvas != null)
        {
            currentHealthBar.healthBarCanvas.renderMode = RenderMode.WorldSpace;
            currentHealthBar.healthBarCanvas.worldCamera = Camera.main;
        }
    }
    
    private void OnHealthChanged(int newHealth)
    {
        if (currentHealthBar != null)
        {
            currentHealthBar.UpdateHealthBar(newHealth, marble.GetMaxHealth());
        }
    }
    
    public void ShowHealthBar()
    {
        if (currentHealthBar != null)
        {
            currentHealthBar.SetVisible(true);
        }
    }
    
    public void HideHealthBar()
    {
        if (currentHealthBar != null)
        {
            currentHealthBar.SetVisible(false);
        }
    }
    
    public void ToggleHealthBar()
    {
        if (currentHealthBar != null)
        {
            // Toggle visibility based on current alpha
            bool isVisible = currentHealthBar.GetComponent<CanvasGroup>().alpha > 0.5f;
            currentHealthBar.SetVisible(!isVisible);
        }
    }
    
    // Method to update team colors if team changes during gameplay
    public void UpdateTeamColors()
    {
        if (currentHealthBar != null && marble != null)
        {
            currentHealthBar.SetTeam(marble.Team());
        }
    }
}

