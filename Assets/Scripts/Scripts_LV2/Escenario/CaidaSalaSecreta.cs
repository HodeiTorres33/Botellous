using UnityEngine;

// Trigger dentro de los pozos que bajan a las salas secretas.
// Si el jugador cae por el pozo, pierde la mitad de la vida.
// Si pasa por aquí subiendo en el ascensor, no le pasa nada.
public class CaidaSalaSecreta : MonoBehaviour
{
    // Pon aquí la vida máxima que tenga el jugador (la del script de Hodei)
    public int vidaMaximaJugador = 6;

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (!otro.CompareTag("Player"))
        {
            return;
        }

        // Si entra por debajo del centro del trigger es que viene subiendo (en el ascensor)
        if (otro.transform.position.y < transform.position.y)
        {
            return;
        }

        int dano = vidaMaximaJugador / 2;
        otro.SendMessage("RecibirDano", dano, SendMessageOptions.DontRequireReceiver);
    }
}
