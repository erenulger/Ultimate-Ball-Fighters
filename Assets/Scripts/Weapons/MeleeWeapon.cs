using System;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    public event Action<int> OnPerkChanged;
    public event Action<float> OnPerkChangedFloat;
    public int damage = 1;
    public float weaponLengthScale = 1f;
    public float triggerCooldown = 0.06f;
    private float triggerCooldownTimer = 0f;
    private Marble owner;

    void Awake()
    {
        owner = GetComponentInParent<Marble>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if enemy melee weapon hit.
        if (other.gameObject.CompareTag("MeleeWeapon") || other.gameObject.CompareTag("DefensiveWeapon"))
        {
            Debug.Log(other.gameObject.tag);
            Marble enemyOwner = other.GetComponentInParent<Marble>();
            if (enemyOwner.Team() != owner.Team())
            {
                owner.FlipSpin();
            }
        }

        // Check if enemy hit.
        else if (other.gameObject.CompareTag("Marble"))
        {
            Marble marble = other.GetComponent<Marble>();
            if(marble.Team() != owner.Team() && triggerCooldownTimer <= 0)
            {
                marble.TakeDamage(damage);
                specialPerk();

                triggerCooldownTimer = triggerCooldown; 
            }
        }
        
    }

    void Update()
    {
        if(triggerCooldownTimer > 0)
        {
            triggerCooldownTimer -= Time.deltaTime;
        }
    }

    public virtual float GetCurrentPerk()
    {
        return 0f;
    }

    public virtual void specialPerk()
    {

    }

    // Protected method to allow child classes to trigger the event
    protected void TriggerPerkChanged(int newPerkValue)
    {
        OnPerkChanged?.Invoke(newPerkValue);
    }

    // Overloaded method for float values
    protected void TriggerPerkChanged(float newPerkValue)
    {
        OnPerkChangedFloat?.Invoke(newPerkValue);
    }
}