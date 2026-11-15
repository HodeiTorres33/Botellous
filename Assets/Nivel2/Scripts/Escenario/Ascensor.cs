using UnityEngine;

// Ascensor para salir de las salas secretas.
// Está abajo esperando; cuando el jugador se sube, sube por el pozo
// y cuando el jugador se baja, vuelve a bajar.
// Necesita dos BoxCollider2D: uno normal (la plataforma) y otro marcado como trigger encima (el detector).
public class Ascensor : MonoBehaviour
{
    public float altura = 14.4f;
    public float velocidad = 3f;

    private Rigidbody2D rb;
    private Vector3 puntoAbajo;
    private Vector3 puntoArriba;
    private bool jugadorEncima = false;

    // Guarda el punto de abajo (donde empieza) y calcula el de arriba
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        puntoAbajo = transform.position;
        puntoArriba = transform.position + new Vector3(0, altura, 0);
    }

    // Sube si el jugador está encima y baja si no
    void FixedUpdate()
    {
        // Si el jugador está encima subimos, si no bajamos
        Vector3 destino = puntoAbajo;
        if (jugadorEncima)
        {
            destino = puntoArriba;
        }

        // MovePosition mueve el Rigidbody empujando al jugador que va encima
        Vector3 siguientePosicion = Vector3.MoveTowards(transform.position, destino, velocidad * Time.fixedDeltaTime);
        rb.MovePosition(siguientePosicion);
    }

    // El jugador se sube al ascensor
    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.CompareTag("Player"))
        {
            jugadorEncima = true;
        }
    }

    // El jugador se baja del ascensor
    void OnTriggerExit2D(Collider2D otro)
    {
        if (otro.CompareTag("Player"))
        {
            jugadorEncima = false;
        }
    }
}
