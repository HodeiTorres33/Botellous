using UnityEngine;

// Caída trampa: un trozo de suelo agrietado que tiembla cuando lo pisa el jugador,
// se rompe y le deja caer a la sala secreta. Al rato vuelve a aparecer.
// Necesita dos BoxCollider2D: uno normal (el suelo) y otro marcado como trigger encima (el detector).
public class SueloFragil : MonoBehaviour
{
    public float tiempoAntesDeRomper = 0.6f;
    public float tiempoParaVolver = 4f;
    public float fuerzaTemblor = 0.06f;

    private BoxCollider2D sueloSolido;
    private SpriteRenderer[] dibujos;
    private Vector3 posicionInicial;
    private bool temblando = false;
    private bool roto = false;
    private float contador = 0f;

    void Start()
    {
        posicionInicial = transform.position;
        dibujos = GetComponentsInChildren<SpriteRenderer>();

        // El primer BoxCollider2D es el suelo (el segundo es el detector)
        sueloSolido = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (temblando)
        {
            // Se mueve un poquito a los lados al azar para que parezca que tiembla
            float temblor = Random.Range(-fuerzaTemblor, fuerzaTemblor);
            transform.position = posicionInicial + new Vector3(temblor, 0, 0);

            contador = contador - Time.deltaTime;
            if (contador <= 0)
            {
                Romper();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (!temblando && !roto && otro.CompareTag("Player"))
        {
            temblando = true;
            contador = tiempoAntesDeRomper;
        }
    }

    void Romper()
    {
        temblando = false;
        roto = true;
        transform.position = posicionInicial;

        // Quitamos el suelo y lo escondemos
        sueloSolido.enabled = false;
        for (int i = 0; i < dibujos.Length; i++)
        {
            dibujos[i].enabled = false;
        }

        Invoke("Volver", tiempoParaVolver);
    }

    void Volver()
    {
        roto = false;
        sueloSolido.enabled = true;
        for (int i = 0; i < dibujos.Length; i++)
        {
            dibujos[i].enabled = true;
        }
    }
}
