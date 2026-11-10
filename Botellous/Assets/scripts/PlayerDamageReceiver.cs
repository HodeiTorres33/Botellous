using UnityEngine;

namespace Botellous
{
    public class PlayerDamageReceiver : MonoBehaviour
    {
        // Vida del jugador; los valores se pueden ajustar desde el Inspector.
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        private bool isDead;

        // Restaura la vida al comenzar la partida.
        private void Awake()
        {
            currentHealth = maxHealth;
        }

        // Resta vida y desactiva al jugador cuando llega a cero.
        public void TakeDamage(float amount)
        {
            if (isDead || amount <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            if (currentHealth <= 0f)
            {
                isDead = true;
                Debug.Log("El jugador ha sido derrotado.", this);
                gameObject.SetActive(false);
            }
        }
    }
}
