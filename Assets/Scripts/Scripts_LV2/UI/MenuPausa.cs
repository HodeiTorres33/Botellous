using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Menú de pausa (GDD 7.1): congela el juego con Time.timeScale = 0.
// Se abre y se cierra con Esc. En el móvil, el botón de pausa de ControlesMovil también "pulsa" Esc.
public class MenuPausa : MonoBehaviour
{
    // Panel oscuro con los botones (empieza desactivado)
    public GameObject panelPausa;
    // Nombre exacto de la escena del menú principal (sin el .unity)
    public string escenaMenuPrincipal = "MenuPrincipal";

    private bool pausado = false;

    void Start()
    {
        panelPausa.SetActive(false);

        // Guardamos en qué nivel está el jugador para poder reanudar desde el menú principal.
        // PlayerPrefs guarda datos pequeños que se mantienen aunque se cierre el juego.
        PlayerPrefs.SetString("UltimoNivel", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }

    void Update()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado != null && teclado.escapeKey.wasPressedThisFrame)
        {
            if (pausado)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }
    }

    public void Pausar()
    {
        pausado = true;
        panelPausa.SetActive(true);
        // Con la escala de tiempo a 0 se para todo lo que usa Time.deltaTime, las físicas y las animaciones
        Time.timeScale = 0f;
        // El sonido no depende del tiempo del juego, así que lo pausamos aparte
        AudioListener.pause = true;
    }

    public void Reanudar()
    {
        pausado = false;
        panelPausa.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void ReiniciarNivel()
    {
        // Hay que devolver el tiempo y el sonido antes de cambiar de escena, si no la nueva empieza congelada
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IrAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(escenaMenuPrincipal);
    }
}
