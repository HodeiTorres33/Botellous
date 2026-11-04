using UnityEngine;

// Plataforma que va y viene entre su posición inicial y otro punto.
public class PlataformaMovil : MonoBehaviour
{
    public Vector3 desplazamiento = new Vector3(6f, 0f, 0f);
    public float velocidad = 2f;

    private Vector3 puntoA;
    private Vector3 puntoB;
    private bool yendoAB = true;

    void Start()
    {
        puntoA = transform.position;
        puntoB = transform.position + desplazamiento;
    }

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

    void OnCollisionEnter2D(Collision2D otro)
    {
        if (otro.gameObject.CompareTag("Player"))
        {
            otro.transform.SetParent(transform);
        }
    }

    void OnCollisionExit2D(Collision2D otro)
    {
        if (otro.gameObject.CompareTag("Player"))
        {
            otro.transform.SetParent(null);
        }
    }
}
