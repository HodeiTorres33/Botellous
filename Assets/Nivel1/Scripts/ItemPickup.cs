
using UnityEngine;

/// <summary>
/// Clase base de los items que sueltan los enemigos.
/// Usa HoldInteract para recoger los objetos manteniendo pulsada la E.
/// Los hijos solo implementan Apply().
/// </summary>
public abstract class ItemPickup : MonoBehaviour
{
    public string playerTag = "Player";

    [Header("Efecto flotante")]
    public Transform visual;
    public float bobHeight = 0.08f;
    public float bobSpeed = 3f;

    [Header("Recogida")]
    [Tooltip("Segundos que hay que mantener pulsado.")]
    public float holdTime = 0.8f;

    [Header("Indicador (E)")]
    [Range(0.04f, 0.6f)]
    public float promptSize = 0.108f;

    [Tooltip("Altura del indicador sobre el item.")]
    public float promptHeight = 0.8f;

    // Componente que muestra la E y detecta cuándo se mantiene pulsada
    HoldInteract interact;
    Vector3 visualStart;
    bool picked;

    // Lo que hace cada objeto al cogerlo (lo escriben DamageItem y HealthItem)
    protected abstract void Apply(PlayerMov player);

    // Añade HoldInteract si no lo tiene y le pasa la configuración del objeto
    void Awake()
    {
        interact = GetComponent<HoldInteract>();

        if (interact == null)
            interact = gameObject.AddComponent<HoldInteract>();

        interact.holdTime = holdTime;
        interact.playerTag = playerTag;
        interact.promptSize = promptSize;
        interact.promptHeight = promptHeight;
    }

    // Se apunta al aviso de 'interactuado' de HoldInteract
    void OnEnable()
    {
        if (interact != null)
            interact.OnInteracted += Pickup;
    }

    // Se desapunta para no recibir avisos cuando el objeto está desactivado
    void OnDisable()
    {
        if (interact != null)
            interact.OnInteracted -= Pickup;
    }

    // Guarda la posición inicial del dibujo para hacerlo flotar alrededor de ella
    void Start()
    {
        if (visual != null)
            visualStart = visual.localPosition;
    }

    // Hace flotar el objeto arriba y abajo con una onda seno
    void Update()
    {
        if (visual != null)
        {
            visual.localPosition =
                visualStart +
                Vector3.up *
                (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
        }
    }

    // Se llama al terminar de mantener la E: aplica el efecto una sola vez y borra el objeto
    void Pickup(PlayerMov player)
    {
        if (picked)
            return;

        picked = true;
        Apply(player);
        Destroy(gameObject);
    }

    /// <summary>
    /// Lo usa PlayerMov para no atacar al pulsar la E:
    /// tocar el indicador no cuenta como ataque.
    /// </summary>
    public static bool IsPointerOverPrompt(Vector2 screenPos)
    {
        return HoldInteract.IsPointerOverPrompt(screenPos);
    }
}
