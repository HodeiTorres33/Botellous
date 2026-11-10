using UnityEngine;

/// <summary>Corazón: cura al jugador (y opcionalmente sube su vida máxima).</summary>
public class HealthItem : ItemPickup
{
    public float healAmount = 25f;
    [Tooltip("Si es mayor que 0, también aumenta la vida máxima.")]
    public float maxHealthBonus = 0f;

    protected override void Apply(PlayerController player)
    {
        if (maxHealthBonus > 0f) player.AddMaxHealth(maxHealthBonus, false);
        player.Heal(healAmount);
    }
}
