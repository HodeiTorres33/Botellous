using UnityEngine;

// Plataforma que va y viene entre su posición inicial y otro punto.
public class PlataformaMovil : MonoBehaviour
{
    // Distancia desde la posición inicial hasta el otro extremo del recorrido
    public Vector3 desplazamiento = new Vector3(6f, 0f, 0f);
    public float velocidad = 2f;

    private Vector3 puntoA;
    private Vector3 puntoB;
    // true = va hacia el punto B, false = vuelve al punto A
    private bool yendoAB = true;

    // Guarda los dos extremos del recorrido
    void Start()
    {
        puntoA = transform.position;
        puntoB = transform.position + desplazamiento;
    }

    // Mueve la plataforma hacia el extremo al que va, a velocidad constante
    void FixedUpdate()
    {
        Vector3 destino = puntoA;
        if (yendoAB)
        {
            destino = puntoB;
        }

        transform.position = Vector3.MoveTowards(transform.position, destino, velocidad * Time.fixedDeltaTime);

        // Al llegar a un extremo, cambia de sentido
        if (transform.position == destino)
        {
            yendoAB = !yendoAB;
        }
    }

    // Si el jugador se sube, lo hacemos hijo de la plataforma para que se mueva con ella
    void OnCollisionEnter2D(Collision2D otro)
    {
        if (otro.gameObject.CompareTag("Player"))
        {
            otro.transform.SetParent(transform);
        }
    }

    // Al bajarse deja de ser hijo de la plataforma
    void OnCollisionExit2D(Collision2D otro)
    {
        if (otro.gameObject.CompareTag("Player"))
        {
            otro.transform.SetParent(null);
        }
    }
}
