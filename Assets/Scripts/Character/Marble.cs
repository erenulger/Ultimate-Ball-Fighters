// CharacterBall.cs
using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Marble : MonoBehaviour
{
    [Header("Team/Stats")]
    public string team = "Red";
    public int maxHealth = 100;
    public int health = 100;

    [Header("Motion")]
    public float spinDegPerSec = 180f;       // silahı çevirmek için
    public float initialVelocity = 150f;
    public Vector2 initialVelocityVector = new Vector2(1f, 0f);

    [Header("Damage Effects")]
    public float hitStopDuration = 0.1f;     // Duration to stop movement when taking damage
    public Color damageFlashColor = Color.red; // Color to flash when taking damage
    public float flashDuration = 0.2f;       // Duration of the color flash

    public event Action<int> OnHealthChanged; // current health
    public event Action OnDeath; // death event

    protected Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Color originalColor;
    Vector2 storedVelocity;
    bool isHitStopped = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = maxHealth;
        
        // Store the original color for damage flash effect
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Start()
    {
        initialVelocityVector.Normalize();
        rb.velocity = initialVelocityVector * initialVelocity;

        OnHealthChanged?.Invoke(health);
    }

    void Update()
    {
        rb.velocity = rb.velocity.normalized * initialVelocity;
        // Only rotate if not hit stopped
        if (!isHitStopped)
        {
            transform.Rotate(0f, 0f, spinDegPerSec * Time.deltaTime);
        }
    }

    public string Team() => team;

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        health = Mathf.Clamp(health, 0, maxHealth);
        
        OnHealthChanged?.Invoke(health);
        
        // Trigger hit stop and damage flash effects
        StartCoroutine(HitStopCoroutine());
        StartCoroutine(DamageFlashCoroutine());
        
        if (health <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        health += healAmount;
        health = Mathf.Clamp(health, 0, maxHealth); // Ensure health doesn't exceed max
        
        Debug.Log($"{gameObject.name} healed to: {health}");
        OnHealthChanged?.Invoke(health);
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        health = Mathf.Clamp(health, 0, maxHealth);
        OnHealthChanged?.Invoke(health);
    }

    private void Die()
    {
        GetComponent<HealthBarManager>().OnDestroy();
        GetComponent<PerkBarManager>().OnDestroy();
        OnDeath?.Invoke();
        
        // You can add death effects here
        // For now, we'll just disable the marble instead of destroying it
        gameObject.SetActive(false);
        
        // If you want to destroy it completely, uncomment the line below:
        // Destroy(gameObject);
    }

    public void FlipSpin() => spinDegPerSec *= -1f;

    // Utility methods for health bar system
    public float GetHealthPercentage() => (float)health / maxHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetCurrentHealth() => health;

    /// <summary>
    /// Coroutine that temporarily stops the marble's movement while preserving momentum
    /// </summary>
    private IEnumerator HitStopCoroutine()
    {
        if (isHitStopped) yield break; // Don't stack hit stops
        
        isHitStopped = true;
        
        // Store current velocity and stop the marble
        storedVelocity = rb.velocity;
        rb.velocity = Vector2.zero;
        
        // Wait for hit stop duration
        yield return new WaitForSeconds(hitStopDuration);
        
        // Restore velocity and allow movement again
        rb.velocity = storedVelocity;
        isHitStopped = false;
    }

    /// <summary>
    /// Coroutine that flashes the marble's sprite color briefly
    /// </summary>
    private IEnumerator DamageFlashCoroutine()
    {
        if (spriteRenderer == null) yield break;
        
        // Flash to damage color
        spriteRenderer.color = damageFlashColor;
        
        // Wait for flash duration
        yield return new WaitForSeconds(flashDuration);
        
        // Return to original color
        spriteRenderer.color = originalColor;
    }

}
