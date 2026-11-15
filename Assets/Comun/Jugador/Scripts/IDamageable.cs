// Interfaz común para todo lo que puede recibir daño (jugador y enemigos del nivel 1).
// Así un enemigo o una botella pueden hacer daño sin saber qué script tiene el objetivo.
public interface IDamageable
{
    // Resta 'amount' de vida al objeto
    void TakeDamage(float amount);
}