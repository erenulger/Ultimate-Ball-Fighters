using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Components")]
    public Image emptyBar;               // The background bar (darker team color)
    public Image fullBar;                // The health bar that shrinks (brighter team color)
    public Canvas healthBarCanvas;       // Canvas for world space health bar
    
    [Header("Settings")]
    public bool followTarget = true;     // Should the health bar follow the marble?
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Offset from the marble
    public bool fadeWhenFullHealth = false; // Hide when at full health
    public float fadeSpeed = 2f;         // How fast to fade in/out


    [System.Serializable]
    public class TeamColors
    {
        public string teamName;
        public Color emptyBarColor;      // Darker background color
        public Color fullBarColor;       // Brighter foreground color
    }
    
    public TeamColors[] teamColorSettings = new TeamColors[]
    {
        new TeamColors { teamName = "Red", emptyBarColor = new Color(0.3f, 0.1f, 0.1f), fullBarColor = new Color(0.9f, 0.2f, 0.2f) },
        new TeamColors { teamName = "Blue", emptyBarColor = new Color(0.1f, 0.1f, 0.3f), fullBarColor = new Color(0.2f, 0.2f, 0.9f) },
        new TeamColors { teamName = "Green", emptyBarColor = new Color(0.1f, 0.3f, 0.1f), fullBarColor = new Color(0.2f, 0.9f, 0.2f) },
        new TeamColors { teamName = "Yellow", emptyBarColor = new Color(0.3f, 0.3f, 0.1f), fullBarColor = new Color(0.9f, 0.9f, 0.2f) }
    };
    
    [Header("Health-based Color Modulation")]
    public bool modulateColorByHealth = true;
    public Color criticalHealthTint = new Color(1f, 0.3f, 0.3f); // Red tint when health is critical
    public float criticalHealthThreshold = 0.25f; // 25%
    
    private Transform target;
    private Camera mainCamera;
    private CanvasGroup canvasGroup;
    private int maxHealth;
    private int currentHealth;
    private float targetAlpha = 1f;
    private string currentTeam;
    private Color baseEmptyColor;
    private Color baseFullColor;
    
    void Awake()
    {
        // Get or add CanvasGroup for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
        mainCamera = Camera.main;
        
        // Ensure we have the required components
        if (emptyBar == null || fullBar == null)
        {
            Debug.LogError("HealthBar: EmptyBar or FullBar is not assigned!");
        }
        
        // Set up the fullBar to use filled type
        if (fullBar != null)
        {
            fullBar.type = Image.Type.Filled;
            fullBar.fillMethod = Image.FillMethod.Horizontal;
        }
    }
    
    void Update()
    {
        if (target != null && followTarget)
        {
            // Position the health bar above the marble
            Vector3 worldPosition = target.position + offset;
            transform.position = worldPosition;
            
        }
        
        // Handle fading
        HandleFading();
    }
    
    public void Initialize(Transform marbleTransform, int maxHealthValue, string team = "Red")
    {
        target = marbleTransform;
        maxHealth = maxHealthValue;
        currentHealth = maxHealthValue;
        currentTeam = team;
        
        // Apply team colors
        ApplyTeamColors(team);
        
        // Initialize health bar
        UpdateHealthBar(maxHealthValue, maxHealthValue);
    }
    
    public void UpdateHealthBar(int newHealth, int maxHealthValue)
    {
        currentHealth = newHealth;
        maxHealth = maxHealthValue;
        
        if (fullBar == null) return;
        
        // Calculate health percentage
        float healthPercentage = (float)currentHealth / maxHealth;
        
        // Update fill amount (this makes the fullBar shrink as health decreases)
        fullBar.fillAmount = healthPercentage;
        
        // Update colors if health-based modulation is enabled
        if (modulateColorByHealth)
        {
            UpdateHealthBasedColors(healthPercentage);
        }
        
        // Determine if we should show/hide the health bar
        if (fadeWhenFullHealth)
        {
            targetAlpha = (healthPercentage >= 1f) ? 0f : 1f;
        }
        else
        {
            targetAlpha = 1f;
        }
    }
    
    private void ApplyTeamColors(string team)
    {
        // Find team colors
        TeamColors teamColor = System.Array.Find(teamColorSettings, t => t.teamName.Equals(team, System.StringComparison.OrdinalIgnoreCase));
        
        if (teamColor != null)
        {
            baseEmptyColor = teamColor.emptyBarColor;
            baseFullColor = teamColor.fullBarColor;
        }
        else
        {
            // Default colors if team not found
            Debug.LogWarning($"Team '{team}' not found in team color settings. Using default colors.");
            baseEmptyColor = new Color(0.2f, 0.2f, 0.2f); // Dark gray
            baseFullColor = new Color(0.8f, 0.8f, 0.8f);  // Light gray
        }
        
        // Apply the colors
        if (emptyBar != null)
            emptyBar.color = baseEmptyColor;
        if (fullBar != null)
            fullBar.color = baseFullColor;
    }
    
    private void UpdateHealthBasedColors(float healthPercentage)
    {
        if (fullBar == null) return;
        
        if (healthPercentage <= criticalHealthThreshold)
        {
            // Blend base color with critical health tint
            Color criticalColor = Color.Lerp(baseFullColor, criticalHealthTint, 0.7f);
            fullBar.color = criticalColor;
        }
        else
        {
            // Use base team color
            fullBar.color = baseFullColor;
        }
    }
    
    private void HandleFading()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        }
    }
    
    public void SetTeam(string team)
    {
        currentTeam = team;
        ApplyTeamColors(team);
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void SetVisible(bool visible)
    {
        targetAlpha = visible ? 1f : 0f;
    }
    
    public void ForceShow()
    {
        targetAlpha = 1f;
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }
    
    public void ForceHide()
    {
        targetAlpha = 0f;
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
    
    // Method to add new team colors at runtime
    public void AddTeamColor(string teamName, Color emptyColor, Color fullColor)
    {
        // Check if team already exists
        for (int i = 0; i < teamColorSettings.Length; i++)
        {
            if (teamColorSettings[i].teamName.Equals(teamName, System.StringComparison.OrdinalIgnoreCase))
            {
                teamColorSettings[i].emptyBarColor = emptyColor;
                teamColorSettings[i].fullBarColor = fullColor;
                return;
            }
        }
        
        // Add new team color
        System.Array.Resize(ref teamColorSettings, teamColorSettings.Length + 1);
        teamColorSettings[teamColorSettings.Length - 1] = new TeamColors
        {
            teamName = teamName,
            emptyBarColor = emptyColor,
            fullBarColor = fullColor
        };
    }
}
