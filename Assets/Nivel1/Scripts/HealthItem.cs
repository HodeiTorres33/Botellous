using UnityEngine;

/// <summary>Corazón: cura al jugador (y opcionalmente sube su vida máxima).</summary>
public class HealthItem : ItemPickup
{
    public float healAmount = 25f;
    [Tooltip("Si es mayor que 0, también aumenta la vida máxima.")]
    public float maxHealthBonus = 0f;

    protected override void Apply(PlayerMov player)
    {
        var vida = player.GetComponent<Botellous.PlayerDamageReceiver>();
        if (vida == null) return;
        if (maxHealthBonus > 0f) vida.AddMaxHealth(maxHealthBonus, false);
        vida.Heal(healAmount);
    }
}
