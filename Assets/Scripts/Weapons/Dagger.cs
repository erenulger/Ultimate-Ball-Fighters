using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Dagger : MeleeWeapon
{
    private int hitCount = 0;
    private bool isDual = false;
    [SerializeField] protected GameObject prefab;

    public override void specialPerk()
    {
        Marble owner = GetComponentInParent<Marble>();
        owner.spinDegPerSec *= 1.1f;
        hitCount++;

        if (hitCount >= 2)
        {
            if (!isDual)
            {
                Transform dualWeapon = transform.parent.parent.GetChild(1);
                dualWeapon.Rotate(0f, 0f, 180f);
                Instantiate(prefab, dualWeapon);

                isDual = true;
            }
        }
        TriggerPerkChanged(GetCurrentPerk());
    }

    public override float GetCurrentPerk()
    {   Marble owner = GetComponentInParent<Marble>();

        return math.abs(owner.spinDegPerSec / 360);
    }


}
