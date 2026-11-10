using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Ponlo directamente en el Tilemap de la puerta.
/// Crea una zona de detección alrededor del tilemap y muestra la E
/// (usando HoldInteract) cuando el jugador se acerca.
/// Al pulsar E imprime en consola; ahí es donde cambias de escena.
/// </summary>
public class SceneInteract : MonoBehaviour
{
    [Header("Interacción")]
    [Tooltip("Segundos que hay que mantener la E.")]
    public float holdTime = 0.3f;

    public string playerTag = "Player";

    [Header("Cambio de escena")]
    [Tooltip("Nombre exacto de la escena a cargar (sin .unity). Tiene que estar en Build Profiles > Scene List.")]
    public string escenaSiguiente = "Nivel2_Catacumbas";

    bool cargando = false;

    [Header("Indicador (E)")]
    [Range(0.04f, 0.6f)]
    public float promptSize = 0.108f;

    [Tooltip("Altura del indicador sobre el centro de la puerta.")]
    public float promptHeight = 0.6f;

    [Header("Zona de detección")]
    [Tooltip("Margen extra alrededor del tilemap donde se activa la E.")]
    public float detectionPadding = 1.5f;

    HoldInteract interact;

    void Awake()
    {
        var tilemap = GetComponent<Tilemap>();

        // Hijo en el centro del tilemap con el trigger y HoldInteract
        var child = new GameObject("DoorInteract");
        child.transform.SetParent(transform, false);

        var col = child.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        if (tilemap != null)
        {
            tilemap.CompressBounds();
            Bounds b = tilemap.localBounds;

            child.transform.localPosition = b.center;
            col.size = new Vector2(
                b.size.x + detectionPadding * 2f,
                b.size.y + detectionPadding * 2f
            );
        }
        else
        {
            col.size = new Vector2(3f, 3f);
        }

        interact = child.AddComponent<HoldInteract>();
        interact.holdTime = holdTime;
        interact.playerTag = playerTag;
        interact.promptSize = promptSize;
        interact.promptHeight = promptHeight;
    }

    void OnEnable()
    {
        if (interact != null)
            interact.OnInteracted += OnInteract;
    }

    void OnDisable()
    {
        if (interact != null)
            interact.OnInteracted -= OnInteract;
    }

    void Start()
    {
        // Por si Awake corrió antes de que OnEnable tuviera interact
        if (interact != null)
        {
            interact.OnInteracted -= OnInteract;
            interact.OnInteracted += OnInteract;
        }
    }

    void OnInteract(PlayerMov player)
    {
        if (cargando) return;
        cargando = true;

        SceneManager.LoadScene(escenaSiguiente);
    }
}