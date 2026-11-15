using UnityEngine;

/// <summary>Corazón: cura al jugador (y opcionalmente sube su vida máxima).</summary>
public class HealthItem : ItemPickup
{
    // Vida que cura al cogerlo
    public float healAmount = 25f;
    [Tooltip("Si es mayor que 0, también aumenta la vida máxima.")]
    public float maxHealthBonus = 0f;

    // Se llama al coger el objeto: busca la vida del jugador y la cura
    protected override void Apply(PlayerMov player)
    {
        var vida = player.GetComponent<Botellous.PlayerDamageReceiver>();
        if (vida == null) return;
        // Si está configurado, primero sube la vida máxima
        if (maxHealthBonus > 0f) vida.AddMaxHealth(maxHealthBonus, false);
        vida.Heal(healAmount);
    }
}
