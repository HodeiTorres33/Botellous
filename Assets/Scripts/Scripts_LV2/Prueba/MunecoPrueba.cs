using UnityEngine;
using UnityEngine.InputSystem;

// Muñeco de prueba con la etiqueta "Player" para ver a los enemigos atacar
// sin tener todavía el personaje principal. NO es el jugador del juego.
// Vuela con las flechas (atraviesa paredes) para acercarlo o alejarlo de los esqueletos.
// Teclas: H = herir a todos los enemigos, K = matar a todos los enemigos.
public class MunecoPrueba : MonoBehaviour
{
    public float velocidad = 6f;
    // Vida de mentira para probar la barra de vida de arriba
    public int vidaMaxima = 6;
    public BarraVida barraVida;

    private SpriteRenderer sr;
    private float tiempoRojo = 0f;
    private int vida;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        vida = vidaMaxima;
        if (barraVida != null)
        {
            barraVida.Actualizar(vida, vidaMaxima);
        }
    }

    void Update()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado == null)
        {
            return;
        }

        // Mover el muñeco con las flechas (vuela, para poder recorrer todo el mapa)
        float movimientoX = 0;
        float movimientoY = 0;
        if (teclado.leftArrowKey.isPressed)
        {
            movimientoX = -1;
        }
        if (teclado.rightArrowKey.isPressed)
        {
            movimientoX = 1;
        }
        if (teclado.upArrowKey.isPressed)
        {
            movimientoY = 1;
        }
        if (teclado.downArrowKey.isPressed)
        {
            movimientoY = -1;
        }
        transform.position = transform.position + new Vector3(movimientoX, movimientoY, 0) * velocidad * Time.deltaTime;

        // Probar las animaciones de herido y morir
        if (teclado.hKey.wasPressedThisFrame)
        {
            DanarEnemigos(1);
        }
        if (teclado.kKey.wasPressedThisFrame)
        {
            DanarEnemigos(100);
        }

        // Vuelve a su color después de recibir un golpe
        tiempoRojo = tiempoRojo - Time.deltaTime;
        if (tiempoRojo <= 0)
        {
            sr.color = Color.white;
        }
    }

    void DanarEnemigos(int cantidad)
    {
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemigo");
        for (int i = 0; i < enemigos.Length; i++)
        {
            enemigos[i].SendMessage("RecibirDano", cantidad, SendMessageOptions.DontRequireReceiver);
        }
    }

    // Los enemigos llaman a esta función cuando le pegan
    public void RecibirDano(int cantidad)
    {
        Debug.Log("El muñeco ha recibido " + cantidad + " de daño");
        sr.color = Color.red;
        tiempoRojo = 0.2f;

        vida = vida - cantidad;
        if (vida <= 0)
        {
            // El muñeco no muere: se rellena la vida para seguir probando
            Debug.Log("El muñeco se habría muerto. Vuelve a tener toda la vida.");
            vida = vidaMaxima;
        }
        if (barraVida != null)
        {
            barraVida.Actualizar(vida, vidaMaxima);
        }
    }

    // Los enemigos llaman a esta función al morir
    public void RecuperarVida(int cantidad)
    {
        Debug.Log("El muñeco recupera " + cantidad + " de vida");
        vida = vida + cantidad;
        // Nunca más de la vida máxima
        if (vida > vidaMaxima)
        {
            vida = vidaMaxima;
        }
        if (barraVida != null)
        {
            barraVida.Actualizar(vida, vidaMaxima);
        }
    }
}
