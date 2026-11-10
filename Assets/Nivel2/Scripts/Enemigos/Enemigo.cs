using UnityEngine;

public enum TipoEnemigo
{
    Lancero,     // 1ª parte: cuerpo a cuerpo, sale del suelo
    Espadachin,  // 2ª parte: espadazo con estocada
    Fusilero     // 3ª parte: dispara de lejos
}

public class Enemigo : MonoBehaviour
{
    public TipoEnemigo tipo = TipoEnemigo.Lancero;

    public int vida = 3;
    // Barra pequeña encima de la cabeza (prefab BarraVidaEnemigo)
    public BarraVida barraVida;
    // Vida que recupera el jugador al matar a este esqueleto
    public int vidaQueDaAlMorir = 1;

    public float velocidad = 1.5f;
    public float distanciaPatrulla = 3f;

    public float distanciaVer = 6f;
    public float alturaVer = 2.5f;
    public float distanciaAtaque = 1.8f;
    public float tiempoEntreAtaques = 1.2f;
    public int dano = 1;

    public bool empiezaEnterrado = true;
    public float distanciaAparecer = 4.5f;

    public float impulsoEstocada = 3f;

    public GameObject prefabBala;
    public float distanciaHuir = 2.5f;
    public float velocidadBala = 9f;
    // Posición de la punta del fusil respecto a los pies (mirando a la derecha)
    public Vector2 puntaFusil = new Vector2(2.3f, 1.78f);

    // Componentes
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Collider2D col;
    private Transform jugador;

    // Estado del enemigo
    private float posicionInicialX;
    private int direccion = 1; // 1 = derecha, -1 = izquierda
    private float contadorAtaque = 0f;
    private bool atacando = false;
    private bool muerto = false;
    private bool enterrado = false;
    private int vidaMaxima;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        posicionInicialX = transform.position.x;

        // Nos guardamos la vida con la que empieza para saber cuánto ocupa la barra llena
        vidaMaxima = vida;
        if (barraVida != null)
        {
            barraVida.Actualizar(vida, vidaMaxima);
        }

        // Buscamos al jugador por su etiqueta
        GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");
        if (objetoJugador != null)
        {
            jugador = objetoJugador.transform;
        }

        // Solo el lancero puede empezar enterrado (su Animator es el único que tiene "Aparecido")
        if (tipo == TipoEnemigo.Lancero)
        {
            enterrado = empiezaEnterrado;
            anim.SetBool("Aparecido", !enterrado);
        }
    }

    void Update()
    {
        if (muerto)
        {
            return;
        }

        // El contador baja con el tiempo, cuando llega a 0 puede volver a atacar
        contadorAtaque = contadorAtaque - Time.deltaTime;

        // Si está enterrado, espera a que el jugador se acerque para salir
        if (enterrado)
        {
            if (jugador != null && Vector2.Distance(transform.position, jugador.position) < distanciaAparecer)
            {
                anim.SetBool("Aparecido", true);
            }
            return;
        }

        if (atacando)
        {
            if (tipo == TipoEnemigo.Espadachin)
            {
                // Durante la estocada va frenando poco a poco
                rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.9f, rb.linearVelocity.y);
            }
            else
            {
                // Los demás se quedan quietos mientras atacan
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
        else if (VeAlJugador())
        {
            PerseguirJugador();
        }
        else
        {
            Patrullar();
        }

        // Le pasamos la velocidad al Animator para que cambie entre Idle y Andar
        anim.SetFloat("Velocidad", Mathf.Abs(rb.linearVelocity.x));
    }

    void Patrullar()
    {
        // Si se ha alejado demasiado del punto inicial, se da la vuelta
        if (transform.position.x > posicionInicialX + distanciaPatrulla)
        {
            direccion = -1;
        }
        if (transform.position.x < posicionInicialX - distanciaPatrulla)
        {
            direccion = 1;
        }

        rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);
        Girar();
    }

    void PerseguirJugador()
    {
        // Miramos hacia el jugador
        if (jugador.position.x > transform.position.x)
        {
            direccion = 1;
        }
        else
        {
            direccion = -1;
        }
        Girar();

        float distancia = Mathf.Abs(jugador.position.x - transform.position.x);

        // El fusilero no quiere pelear de cerca: si el jugador se acerca mucho, se aleja
        if (tipo == TipoEnemigo.Fusilero && distancia < distanciaHuir)
        {
            direccion = -direccion;
            Girar();
            rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);
            return;
        }

        if (distancia > distanciaAtaque)
        {
            // Todavía está lejos: andamos hacia él
            rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);
        }
        else
        {
            // Está a distancia de ataque: paramos y atacamos si ya ha pasado el tiempo
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (contadorAtaque <= 0)
            {
                Atacar();
            }
        }
    }

    void Atacar()
    {
        atacando = true;
        contadorAtaque = tiempoEntreAtaques;
        anim.SetTrigger("Atacar");

        if (tipo == TipoEnemigo.Espadachin)
        {
            // Pequeño salto hacia delante al dar el espadazo
            rb.linearVelocity = new Vector2(direccion * impulsoEstocada, rb.linearVelocity.y);
        }
    }

    bool VeAlJugador()
    {
        if (jugador == null)
        {
            return false;
        }

        float distanciaX = Mathf.Abs(jugador.position.x - transform.position.x);
        float distanciaY = Mathf.Abs(jugador.position.y - transform.position.y);

        // Lo ve si está cerca y más o menos a su misma altura
        return distanciaX < distanciaVer && distanciaY < alturaVer;
    }

    void Girar()
    {
        // Los sprites miran a la derecha, así que si va a la izquierda los volteamos
        sr.flipX = direccion == -1;
    }

    // ---------- Ataques ----------

    // Lancero y espadachín: si el jugador sigue cerca cuando llega el golpe, le hace daño
    void GolpeCuerpoACuerpo()
    {
        float distancia = Vector2.Distance(transform.position, jugador.position);
        if (distancia < distanciaAtaque + 0.5f)
        {
            jugador.SendMessage("RecibirDano", dano, SendMessageOptions.DontRequireReceiver);
        }
    }

    // Fusilero: crea una bala en la punta del fusil
    void Disparar()
    {
        Vector3 posicionBala = transform.position + new Vector3(puntaFusil.x * direccion, puntaFusil.y, 0);
        GameObject bala = Instantiate(prefabBala, posicionBala, Quaternion.identity);

        BalaFusil scriptBala = bala.GetComponent<BalaFusil>();
        scriptBala.velocidad = velocidadBala * direccion;
        scriptBala.dano = dano;
    }

    // ---------- Eventos de animación (se llaman desde los clips) ----------

    // Fotograma del golpe: la lanza estirada, el espadazo o el fogonazo del fusil
    public void EventoGolpe()
    {
        if (jugador == null || muerto)
        {
            return;
        }

        if (tipo == TipoEnemigo.Fusilero)
        {
            Disparar();
        }
        else
        {
            GolpeCuerpoACuerpo();
        }
    }

    // Último fotograma del ataque
    public void EventoFinAtaque()
    {
        atacando = false;
    }

    // Último fotograma de la animación de salir del suelo (solo lancero)
    public void EventoAparecido()
    {
        enterrado = false;
        contadorAtaque = 0.5f;
    }

    // Último fotograma de la animación de morir
    public void EventoMuerteTerminada()
    {
        Destroy(gameObject, 1f);
    }

    // ---------- Daño ----------

    // El jugador (o una trampa) llama a esta función cuando le hace daño
    public void RecibirDano(int cantidad)
    {
        if (muerto || enterrado)
        {
            return;
        }

        vida = vida - cantidad;
        atacando = false;

        if (barraVida != null)
        {
            barraVida.Actualizar(vida, vidaMaxima);
        }

        if (vida <= 0)
        {
            Morir();
        }
        else
        {
            anim.SetTrigger("Herido");
        }
    }

    void Morir()
    {
        muerto = true;
        anim.SetBool("Muerto", true);

        // Un esqueleto muerto ya no necesita barra
        if (barraVida != null)
        {
            barraVida.Ocultar();
        }

        // Premio por matarlo: el jugador recupera un poco de vida.
        // El script del jugador tiene que tener una función RecuperarVida(int)
        if (jugador != null)
        {
            jugador.SendMessage("RecuperarVida", vidaQueDaAlMorir, SendMessageOptions.DontRequireReceiver);
        }

        // Quitamos las físicas para que el cadáver no estorbe
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        col.enabled = false;
    }
}
