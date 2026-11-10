using UnityEngine;
using UnityEngine.UI;

// Barra de vida reutilizable: sirve para el jugador (arriba en la pantalla)
// y para los enemigos (encima de la cabeza).
// Quien tenga la vida llama a Actualizar(vidaActual, vidaMaxima) cada vez que cambia.
public class BarraVida : MonoBehaviour
{
    // Image con "Image Type" en Filled y "Fill Method" en Horizontal
    public Image relleno;
    // Texto "4 / 6" (se puede dejar vacío)
    public Text texto;
    // Para los enemigos: la barra no se ve hasta que les haces daño
    public bool ocultarSiEstaLlena = false;

    // Rellena la barra según la vida y escribe el texto (si lo hay)
    public void Actualizar(int vidaActual, int vidaMaxima)
    {
        // fillAmount va de 0 (vacía) a 1 (llena)
        relleno.fillAmount = (float)vidaActual / vidaMaxima;

        if (texto != null)
        {
            texto.text = vidaActual + " / " + vidaMaxima;
        }

        if (ocultarSiEstaLlena)
        {
            gameObject.SetActive(vidaActual < vidaMaxima);
        }
    }

    // Esconde la barra (por ejemplo, cuando muere el enemigo)
    public void Ocultar()
    {
        gameObject.SetActive(false);
    }
}
