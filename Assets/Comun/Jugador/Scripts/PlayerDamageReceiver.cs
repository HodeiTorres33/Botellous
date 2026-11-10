using UnityEngine;
using UnityEngine.SceneManagement;

namespace Botellous
{
    public class PlayerDamageReceiver : MonoBehaviour, IDamageable
    {
        // Vida del jugador; los valores se pueden ajustar desde el Inspector.
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private int puntosNivel2 = 5;
        [SerializeField] private BarraVida barraVida;
        [SerializeField] private float segundosReinicio = 2f;

        private bool isDead;
        private SpriteRenderer spriteRenderer;
        private float tiempoRojo = 0f;

        // Restaura la vida al comenzar la partida.
        private void Awake()
        {
            currentHealth = maxHealth;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            ActualizarBarra();
        }

        private void Update()
        {
            tiempoRojo = tiempoRojo - Time.deltaTime;
            if (tiempoRojo <= 0 && spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }
        }

        // Resta vida y reinicia el nivel cuando llega a cero.
        public void TakeDamage(float amount)
        {
            if (isDead || amount <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            ActualizarBarra();

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red;
                tiempoRojo = 0.15f;
            }

            if (currentHealth <= 0f)
            {
                isDead = true;
                Debug.Log("El jugador ha sido derrotado.", this);
                GetComponent<PlayerMov>().QuitarMejorasDelNivel();
                GetComponent<PlayerMov>().enabled = false;
                GetComponent<Rigidbody2D>().simulated = false;
                Invoke("ReiniciarNivel", segundosReinicio);
            }
        }

        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            ActualizarBarra();
        }

        public void AddMaxHealth(float amount, bool alsoHeal)
        {
            maxHealth = maxHealth + amount;
            if (alsoHeal)
            {
                currentHealth = currentHealth + amount;
            }
            ActualizarBarra();
        }

        public void RecibirDano(int puntos)
        {
            TakeDamage(puntos * maxHealth / puntosNivel2);
        }

        public void RecuperarVida(int puntos)
        {
            Heal(puntos * maxHealth / puntosNivel2);
        }

        private void ActualizarBarra()
        {
            if (barraVida != null)
            {
                int puntos = Mathf.CeilToInt(currentHealth / maxHealth * puntosNivel2);
                barraVida.Actualizar(puntos, puntosNivel2);
            }
        }

        private void ReiniciarNivel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
