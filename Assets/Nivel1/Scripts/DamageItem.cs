using UnityEngine;

/// <summary>Daga: aumenta el daño base del jugador.</summary>
public class DamageItem : ItemPickup
{
    public float damageBonus = 5f;

    protected override void Apply(PlayerMov player)
    {
        player.AddDamage(damageBonus);
    }
}
