using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menú principal (portada del juego). Los botones Jugar, Opciones, Créditos y Salir
// están dibujados en la imagen de fondo; encima hay botones transparentes que llaman a estas funciones.
public class MenuPrincipal : MonoBehaviour
{
    // Nombre exacto de la escena con la que empieza el juego (sin el .unity)
    public string escenaJuego = "Level1Joel";
    public GameObject panelOpciones;
    public GameObject panelCreditos;
    // Botón "Reanudar partida" del panel de opciones
    public Button botonReanudar;

    // Prepara el menú: tiempo normal, paneles cerrados y botón de reanudar
    void Start()
    {
        // Por si venimos del menú de pausa con el juego congelado
        Time.timeScale = 1f;
        CerrarPaneles();

        // Si nunca se ha jugado no hay nada que reanudar, así que el botón sale desactivado
        botonReanudar.interactable = PlayerPrefs.HasKey("UltimoNivel");
    }

    // Carga el último nivel en el que estuvo el jugador (lo guarda MenuPausa al empezar cada nivel)
    public void Reanudar()
    {
        SceneManager.LoadScene(PlayerPrefs.GetString("UltimoNivel"));
    }

    // Empieza una partida nueva desde el nivel 1
    public void Jugar()
    {
        // Partida nueva: el jugador empieza sin dagas
        PlayerMov.danoExtra = 0f;
        SceneManager.LoadScene(escenaJuego);
    }

    // Enseña el panel de opciones (volumen, pantalla completa, reanudar)
    public void AbrirOpciones()
    {
        panelOpciones.SetActive(true);
    }

    // Enseña el panel de créditos
    public void AbrirCreditos()
    {
        panelCreditos.SetActive(true);
    }

    // Botón "Volver" de los dos paneles
    public void CerrarPaneles()
    {
        panelOpciones.SetActive(false);
        panelCreditos.SetActive(false);
    }

    // Slider de volumen (de 0 a 1): cambia el volumen de todo el juego
    public void CambiarVolumen(float volumen)
    {
        AudioListener.volume = volumen;
    }

    // Casilla de pantalla completa
    public void CambiarPantallaCompleta(bool activada)
    {
        Screen.fullScreen = activada;
    }

    // Cierra el juego
    public void Salir()
    {
        Debug.Log("Salir del juego");
        Application.Quit(); // en el editor no hace nada, solo funciona en el juego ya compilado
    }
}
