using UnityEngine;
using UnityEngine.SceneManagement;

// Puerta del final del nivel 2: al tocarla el jugador pasa directamente al nivel 3 (el de Hodei).
public class FinNivel : MonoBehaviour
{
    // Nombre exacto de la escena del nivel 3 (sin el .unity)
    public string escenaSiguiente = "Nivel3";

    private bool terminado = false;

    // Al llegar el jugador carga el nivel 3
    void OnTriggerEnter2D(Collider2D otro)
    {
        // Solo cuenta la primera vez que entra el jugador
        if (terminado || !otro.CompareTag("Player"))
        {
            return;
        }

        terminado = true;
        // La escena tiene que estar en File > Build Profiles > Scene List
        SceneManager.LoadScene(escenaSiguiente);
    }
}
