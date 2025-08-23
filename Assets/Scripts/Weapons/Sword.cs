using UnityEngine;
using System;


public class Sword : MeleeWeapon
{
    public override void specialPerk()
    {
        damage += 1;
        TriggerPerkChanged(GetCurrentPerk());
    }
    public override float GetCurrentPerk()
    {
        return damage;
    }
}