using UnityEngine;
using UnityEngine.Rendering.Universal;

// Hace que la luz de la antorcha tiemble como si fuera fuego de verdad.
public class Antorcha : MonoBehaviour
{
    public float intensidadMinima = 1.0f;
    public float intensidadMaxima = 1.6f;
    public float rapidez = 6f;

    private Light2D luz;
    private float semilla;

    void Start()
    {
        luz = GetComponent<Light2D>();
        // Cada antorcha empieza en un punto distinto para que no parpadeen todas a la vez
        semilla = Random.Range(0f, 100f);
    }

    void Update()
    {
        // PerlinNoise da un número entre 0 y 1 que cambia suave, perfecto para fuego
        float ruido = Mathf.PerlinNoise(semilla, Time.time * rapidez);
        luz.intensity = Mathf.Lerp(intensidadMinima, intensidadMaxima, ruido);
    }
}
