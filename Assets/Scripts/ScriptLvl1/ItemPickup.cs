
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

    HoldInteract interact;
    Vector3 visualStart;
    bool picked;

    protected abstract void Apply(PlayerController player);

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

    void OnEnable()
    {
        if (interact != null)
            interact.OnInteracted += Pickup;
    }

    void OnDisable()
    {
        if (interact != null)
            interact.OnInteracted -= Pickup;
    }

    void Start()
    {
        if (visual != null)
            visualStart = visual.localPosition;
    }

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

    void Pickup(PlayerController player)
    {
        if (picked)
            return;

        picked = true;
        Apply(player);
        Destroy(gameObject);
    }

    /// <summary>
    /// Compatibilidad con PlayerController:
    /// tocar el indicador no cuenta como ataque.
    /// </summary>
    public static bool IsPointerOverPrompt(Vector2 screenPos)
    {
        return HoldInteract.IsPointerOverPrompt(screenPos);
    }
}
