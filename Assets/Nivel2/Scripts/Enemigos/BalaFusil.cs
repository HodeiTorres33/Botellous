using UnityEngine;

// Bala que dispara el esqueleto fusilero.
// Avanza en línea recta, daña al jugador y desaparece al chocar con algo.
public class BalaFusil : MonoBehaviour
{
    public float velocidad = 9f;
    public int dano = 1;
    public float tiempoVida = 4f;

    void Start()
    {
        // Si va hacia la izquierda, giramos el dibujo
        if (velocidad < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }

        // Se destruye sola pasado un tiempo por si no choca con nada
        Destroy(gameObject, tiempoVida);
    }

    void Update()
    {
        transform.position = transform.position + new Vector3(velocidad * Time.deltaTime, 0, 0);
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        // Las balas no dañan a los enemigos
        if (otro.CompareTag("Enemigo"))
        {
            return;
        }

        if (otro.CompareTag("Player"))
        {
            otro.SendMessage("RecibirDano", dano, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
        else if (!otro.isTrigger)
        {
            // Ha chocado con una pared o con el suelo
            Destroy(gameObject);
        }
    }
}
