using UnityEngine;

/// <summary>Daga: aumenta el daño base del jugador.</summary>
public class DamageItem : ItemPickup
{
    // Daño que suma al ataque del jugador
    public float damageBonus = 5f;

    // Se llama al coger el objeto (mantener la E): sube el daño del jugador
    protected override void Apply(PlayerMov player)
    {
        player.AddDamage(damageBonus);
    }
}
