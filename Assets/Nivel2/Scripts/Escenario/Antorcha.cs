using UnityEngine;
using UnityEngine.Rendering.Universal;

// Hace que la luz de la antorcha tiemble como si fuera fuego de verdad.
public class Antorcha : MonoBehaviour
{
    // La intensidad de la luz va cambiando entre estos dos valores
    public float intensidadMinima = 1.0f;
    public float intensidadMaxima = 1.6f;
    // Cuanto más alto, más rápido tiembla la luz
    public float rapidez = 6f;

    private Light2D luz;
    private float semilla;

    // Coge la luz y elige un punto de partida al azar para el parpadeo
    void Start()
    {
        luz = GetComponent<Light2D>();
        // Cada antorcha empieza en un punto distinto para que no parpadeen todas a la vez
        semilla = Random.Range(0f, 100f);
    }

    // Cambia la intensidad de la luz suavemente, como el fuego
    void Update()
    {
        // PerlinNoise da un número entre 0 y 1 que cambia suave, perfecto para fuego
        float ruido = Mathf.PerlinNoise(semilla, Time.time * rapidez);
        luz.intensity = Mathf.Lerp(intensidadMinima, intensidadMaxima, ruido);
    }
}
