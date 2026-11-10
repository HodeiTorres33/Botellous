using UnityEngine;

// Controles táctiles para Android. Cada botón tiene un "On-Screen Button" del Input System
// que imita una tecla del ordenador (A, D, W, Esc o el clic izquierdo),
// así el script del jugador funciona igual en el PC y en el móvil sin cambiar nada.
// En el PC los botones se esconden solos.
public class ControlesMovil : MonoBehaviour
{
    // Márcalo para ver los botones en el editor mientras pruebas
    public bool verEnElOrdenador = false;

    // En el PC se esconden los botones táctiles (salvo que 'verEnElOrdenador' esté marcado)
    void Start()
    {
        // Application.isMobilePlatform es true en Android, iOS y en el Device Simulator de Unity
        if (!Application.isMobilePlatform && !verEnElOrdenador)
        {
            gameObject.SetActive(false);
        }
    }
}
