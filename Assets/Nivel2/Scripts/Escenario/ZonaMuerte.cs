using UnityEngine;

// Trampa mortal: los pinchos del fondo del foso.
// Todo lo que cae aquí (jugador o enemigo) recibe tanto daño que muere.
public class ZonaMuerte : MonoBehaviour
{
    // Daño enorme para que mate sin importar la vida que tenga
    public int dano = 999;

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.CompareTag("Player") || otro.CompareTag("Enemigo"))
        {
            // El jugador y los enemigos tienen la función RecibirDano(int)
            otro.SendMessage("RecibirDano", dano, SendMessageOptions.DontRequireReceiver);
        }
    }
}
