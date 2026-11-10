using UnityEngine;
using UnityEngine.SceneManagement;

// Puerta del final del nivel 2. 
public class FinNivel : MonoBehaviour
{
    // Nombre exacto de la escena del nivel 3 (sin el .unity)
    public string escenaSiguiente = "Nivel3";
    public float segundosEspera = 3f;
    // Panel con el texto "Estás cerca del centro de la fábrica..." (empieza desactivado)
    public GameObject textoFinal;

    private bool terminado = false;

    // El texto final empieza escondido
    void Start()
    {
        if (textoFinal != null)
        {
            textoFinal.SetActive(false);
        }
    }

    // Al llegar el jugador: enseña el texto y carga el nivel 3 tras unos segundos
    void OnTriggerEnter2D(Collider2D otro)
    {
        // Solo cuenta la primera vez que entra el jugador
        if (terminado || !otro.CompareTag("Player"))
        {
            return;
        }

        terminado = true;

        if (textoFinal != null)
        {
            textoFinal.SetActive(true);
        }

        // Esperamos unos segundos para que dé tiempo a leer el texto
        Invoke("CargarSiguienteNivel", segundosEspera);
    }

    // Carga la escena del siguiente nivel
    void CargarSiguienteNivel()
    {
        // La escena tiene que estar en File > Build Profiles > Scene List
        SceneManager.LoadScene(escenaSiguiente);
    }
}
