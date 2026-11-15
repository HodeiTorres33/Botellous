using UnityEngine;
using UnityEngine.SceneManagement;

namespace Botellous
{
    // Vida del jugador. La usan los 3 niveles, aunque cada uno hace daño de una forma:
    //  - Nivel 1 y nivel 3: los enemigos y el jefe llaman a TakeDamage (con decimales).
    //  - Nivel 2: los esqueletos, balas y trampas mandan "RecibirDano" con SendMessage (puntos enteros).
    // Implementa IDamageable para que los enemigos del nivel 1 lo puedan encontrar.
    public class PlayerDamageReceiver : MonoBehaviour, IDamageable
    {
        // Vida del jugador; los valores se pueden ajustar desde el Inspector.
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        // La barra de vida tiene 5 segmentos: 1 punto del nivel 2 = 1 segmento = maxHealth / 5
        [SerializeField] private int puntosNivel2 = 5;
        [SerializeField] private BarraVida barraVida;
        // Segundos que tarda en reiniciarse el nivel al morir
        [SerializeField] private float segundosReinicio = 2f;

        private bool isDead;
        private SpriteRenderer spriteRenderer;
        // Tiempo que le queda al jugador para seguir en rojo tras un golpe
        private float tiempoRojo = 0f;

        // Restaura la vida al comenzar la partida.
        private void Awake()
        {
            currentHealth = maxHealth;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Pone la barra llena al empezar el nivel
        private void Start()
        {
            ActualizarBarra();
        }

        // Vuelve a su color normal cuando se acaba el tiempo en rojo
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
            // Si ya está muerto o el daño no es válido no hacemos nada
            if (isDead || amount <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            ActualizarBarra();

            // Se pone rojo un momento para que se note el golpe
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red;
                tiempoRojo = 0.15f;
            }

            if (currentHealth <= 0f)
            {
                isDead = true;
                Debug.Log("El jugador ha sido derrotado.", this);
                // Quita las dagas cogidas en este intento para que no se dupliquen al reiniciar
                GetComponent<PlayerMov>().QuitarMejorasDelNivel();
                // Lo dejamos quieto pero activo, porque la cámara es hija del jugador
                // (si lo desactivamos, la pantalla se queda en negro)
                GetComponent<PlayerMov>().enabled = false;
                GetComponent<Rigidbody2D>().simulated = false;
                Invoke("ReiniciarNivel", segundosReinicio);
            }
        }

        // Cura sin pasar de la vida máxima (corazón del nivel 1 y al matar enemigos).
        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            ActualizarBarra();
        }

        // Sube la vida máxima; si alsoHeal es true también cura esa cantidad (corazón del nivel 1).
        public void AddMaxHealth(float amount, bool alsoHeal)
        {
            maxHealth = maxHealth + amount;
            if (alsoHeal)
            {
                currentHealth = currentHealth + amount;
            }
            ActualizarBarra();
        }

        // Nivel 2: los esqueletos, las balas y las trampas quitan puntos de vida.
        // Pasamos los puntos a vida (con 100 de vida y 5 puntos, 1 punto = 20).
        public void RecibirDano(int puntos)
        {
            TakeDamage(puntos * maxHealth / puntosNivel2);
        }

        // Nivel 2: al matar un esqueleto se recuperan puntos de vida.
        public void RecuperarVida(int puntos)
        {
            Heal(puntos * maxHealth / puntosNivel2);
        }

        // La barra va por segmentos, así que pasamos la vida a segmentos (redondeando hacia arriba)
        private void ActualizarBarra()
        {
            if (barraVida != null)
            {
                int puntos = Mathf.CeilToInt(currentHealth / maxHealth * puntosNivel2);
                barraVida.Actualizar(puntos, puntosNivel2);
            }
        }

        // Vuelve a cargar la escena actual (se llama con Invoke al morir)
        private void ReiniciarNivel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
