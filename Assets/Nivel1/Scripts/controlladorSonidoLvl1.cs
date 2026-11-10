using System.Collections;
using UnityEngine;

/// <summary>
/// ÚNICO controlador de audio del Nivel 1.
/// - Reproduce la introducción (IntroLvl1) y, al terminar, el ambiente en bucle (fondoLvl1).
/// - Reproduce los efectos (ataque enemigo, rotura de botella) cuando otros scripts lo solicitan.
/// Los demás scripts NO tocan ningún AudioSource: solo llaman a los métodos públicos de aquí.
///
/// Uso desde otros scripts:
///     if (controladorSonidoLvl1.Instance != null)
///         controladorSonidoLvl1.Instance.ReproducirAtaqueEnemigo();
/// </summary>
public class controladorSonidoLvl1 : MonoBehaviour
{
    public static controladorSonidoLvl1 Instance { get; private set; }

    [Header("Clips (Assets/sound)")]
    public AudioClip introLvl1;      // IntroLvl1.mp3
    public AudioClip fondoLvl1;      // fondoLvl1.mp3
    public AudioClip atackEnemy;     // atackEnemy.mp3
    public AudioClip botellaRota;    // botellaRota.mp3

    [Header("AudioSources (opcionales: si se dejan vacíos se crean solos)")]
    public AudioSource introSource;
    public AudioSource ambienteSource;
    public AudioSource efectosSource;

    [Header("Volumen")]
    [Range(0f, 1f)] public float volumenGeneral = 1f;
    [Range(0f, 1f)] public float volumenIntro = 1f;
    [Range(0f, 1f)] public float volumenAmbiente = 0.5f;
    [Range(0f, 1f)] public float volumenEfectos = 1f;

    [Header("Flujo de la introducción")]
    [Tooltip("Reproduce la introducción automáticamente al cargar la escena.")]
    public bool reproducirIntroAlIniciar = true;
    [Tooltip("Congela la escena (Time.timeScale = 0) y bloquea al jugador mientras suena la introducción.")]
    public bool pausarEscenaDuranteIntro = true;
    [Tooltip("Opcional. Si se deja vacío se busca el jugador (PlayerMov) de la escena.")]
    public PlayerMov jugador;

    [Header("Anti-solapamiento de efectos")]
    [Tooltip("Segundos mínimos entre dos reproducciones del mismo efecto.")]
    public float intervaloMinimoEfecto = 0.08f;

    bool introYaReproducida;
    bool escenaPausada;
    float timeScaleAnterior = 1f;
    bool jugadorEstabaActivo;
    Coroutine rutinaIntro;
    float ultimoAtaqueEnemigo = -999f;
    float ultimaBotellaRota = -999f;

    // ---------------- CICLO DE VIDA ----------------
    void Awake()
    {
        // Un solo controlador por escena.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        introSource = PrepararSource(introSource, false);
        ambienteSource = PrepararSource(ambienteSource, true);
        efectosSource = PrepararSource(efectosSource, false);
        AplicarVolumenes();
    }

    void Start()
    {
        if (reproducirIntroAlIniciar) ReproducirIntro();
    }

    void OnDestroy()
    {
        // Por seguridad: si la escena se recarga durante la intro, no dejar el juego congelado.
        ReanudarEscena();
        if (Instance == this) Instance = null;
    }

    void OnValidate()
    {
        if (Application.isPlaying && Instance == this) AplicarVolumenes();
    }

    // ---------------- INTRO Y AMBIENTE ----------------
    /// <summary>
    /// Reproduce la introducción una sola vez. El ambiente queda programado para
    /// empezar justo cuando acabe, de modo que nunca se superponen.
    /// </summary>
    public void ReproducirIntro()
    {
        if (introYaReproducida) return;
        introYaReproducida = true;

        if (introLvl1 == null)
        {
            ReproducirAmbiente(); // sin intro, empieza directamente el ambiente
            return;
        }

        ambienteSource.Stop();

        // La escena se congela ANTES de arrancar el audio y de forma síncrona.
        if (pausarEscenaDuranteIntro) PausarEscena();

        introSource.clip = introLvl1;
        introSource.loop = false;
        introSource.Play();

        // Se espera la duración del clip en tiempo REAL (no depende de timeScale ni de isPlaying,
        // que puede ser false al instante si el mp3 aún se está cargando).
        if (pausarEscenaDuranteIntro) rutinaIntro = StartCoroutine(ReanudarTrasIntro(introLvl1.length));

        if (fondoLvl1 != null)
        {
            ambienteSource.clip = fondoLvl1;
            ambienteSource.loop = true;
            ambienteSource.PlayScheduled(AudioSettings.dspTime + introLvl1.length);
        }
    }

    /// <summary>Corta la introducción (p. ej. si se salta) y empieza el ambiente.</summary>
    public void DetenerIntro()
    {
        introYaReproducida = true;
        introSource.Stop();
        ambienteSource.Stop(); // cancela el arranque programado
        if (rutinaIntro != null) { StopCoroutine(rutinaIntro); rutinaIntro = null; }
        ReanudarEscena();
        ReproducirAmbiente();
    }

    /// <summary>Inicia el ambiente en bucle (si ya está sonando, no hace nada).</summary>
    public void ReproducirAmbiente()
    {
        if (fondoLvl1 == null || ambienteSource.isPlaying) return;
        if (introSource.isPlaying) return; // no pisar la introducción

        ambienteSource.clip = fondoLvl1;
        ambienteSource.loop = true;
        ambienteSource.Play();
    }

    public void DetenerAmbiente() => ambienteSource.Stop();

    // ---------------- PAUSA DURANTE LA INTRO ----------------
    IEnumerator ReanudarTrasIntro(float segundos)
    {
        yield return new WaitForSecondsRealtime(segundos);
        rutinaIntro = null;
        ReanudarEscena();
    }

    void PausarEscena()
    {
        if (escenaPausada) return;
        escenaPausada = true;

        timeScaleAnterior = Time.timeScale;
        Time.timeScale = 0f; // congela física, enemigos, botellas, cooldowns y animaciones
        Debug.Log("[controladorSonidoLvl1] Escena pausada durante la introducción.");

        // Con timeScale 0 el Update del jugador sigue ejecutándose (leería el clic de ataque),
        // por eso se desactiva el componente mientras dura la intro.
        if (jugador == null) jugador = FindAnyObjectByType<PlayerMov>();
        if (jugador != null)
        {
            jugadorEstabaActivo = jugador.enabled;
            jugador.enabled = false;
        }
    }

    void ReanudarEscena()
    {
        if (!escenaPausada) return;
        escenaPausada = false;

        Time.timeScale = timeScaleAnterior;
        if (jugador != null) jugador.enabled = jugadorEstabaActivo;
        Debug.Log("[controladorSonidoLvl1] Escena reanudada.");
        // El ambiente ya está programado con PlayScheduled (o lo arranca DetenerIntro).
    }

    // ---------------- EFECTOS ----------------
    /// <summary>Un enemigo ejecuta realmente un ataque.</summary>
    public void ReproducirAtaqueEnemigo()
    {
        ReproducirEfecto(atackEnemy, ref ultimoAtaqueEnemigo);
    }

    /// <summary>Una botella se rompe.</summary>
    public void ReproducirBotellaRota()
    {
        ReproducirEfecto(botellaRota, ref ultimaBotellaRota);
    }

    void ReproducirEfecto(AudioClip clip, ref float ultimaVez)
    {
        if (clip == null) return;
        if (Time.unscaledTime - ultimaVez < intervaloMinimoEfecto) return;

        ultimaVez = Time.unscaledTime;
        efectosSource.PlayOneShot(clip, volumenEfectos);
    }

    // ---------------- VOLUMEN ----------------
    public void SetVolumenGeneral(float v)
    {
        volumenGeneral = Mathf.Clamp01(v);
        AplicarVolumenes();
    }

    void AplicarVolumenes()
    {
        if (introSource != null) introSource.volume = volumenIntro * volumenGeneral;
        if (ambienteSource != null) ambienteSource.volume = volumenAmbiente * volumenGeneral;
        // Los efectos se reproducen con PlayOneShot(clip, volumenEfectos): el general va en el source.
        if (efectosSource != null) efectosSource.volume = volumenGeneral;
    }

    // ---------------- UTILIDADES ----------------
    AudioSource PrepararSource(AudioSource source, bool loop)
    {
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f; // sonido 2D
        return source;
    }
}