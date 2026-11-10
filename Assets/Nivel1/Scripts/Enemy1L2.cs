using System.Collections;
using UnityEngine;

/// <summary>
/// Enemigo lanzador de botellas. Se queda quieto, tiene un cono de visión y,
/// cuando ve al jugador (con línea de visión libre), le lanza una botella
/// apuntando a su posición.
/// </summary>
public class Enemy1L2 : EnemyLevel1
{
    [Header("Visión")]
    public float visionRange = 8f;
    [Range(10f, 360f)] public float visionAngle = 120f;
    [Tooltip("Posición de los ojos respecto al centro (la X se invierte según hacia dónde mire).")]
    public Vector2 eyeOffset = new Vector2(0.1f, 0.3f);
    [Tooltip("Capas que pueden tapar la visión (suelo, paredes...).")]
    public LayerMask obstacleLayers = ~0;
    public bool startFacingRight = false;

    [Header("Lanzamiento")]
    public GameObject bottlePrefab;
    public float bottleDamage = 15f;
    public float throwCooldown = 2f;
    [Tooltip("Segundos desde que empieza la animación hasta que suelta la botella.")]
    public float throwRelease = 0.3f;
    [Tooltip("Dónde aparece la botella (la X se invierte según hacia dónde mire).")]
    public Vector2 throwPoint = new Vector2(0.3f, 0.25f);
    [Tooltip("Velocidad horizontal aproximada de la botella (define el tiempo de vuelo).")]
    public float bottleSpeed = 9f;

    [Header("Animación")]
    public Sprite idleSprite;
    public Sprite[] throwFrames;
    public float throwFps = 10f;

    // Referencias al jugador (se buscan por su etiqueta)
    Transform player;
    Collider2D playerCollider;
    // Tiempo que falta para poder lanzar otra botella
    float cooldownTimer;
    bool throwing;

    // Prepara lo común de los enemigos y hacia dónde mira al empezar
    protected override void Awake()
    {
        base.Awake();
        facing = startFacingRight ? 1f : -1f;
    }

    // Mira hacia su lado inicial y enseña el sprite de estar quieto
    void Start()
    {
        SetFacing(facing);
        ShowSprite(idleSprite);
    }

    // No se mueve en horizontal, solo le afecta la gravedad
    void FixedUpdate()
    {
        if (dead) return;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // se queda en su sitio
    }

    // Si ve al jugador, se gira hacia él y lanza una botella cuando acaba la espera
    protected override void Update()
    {
        base.Update();
        if (dead) return;

        cooldownTimer -= Time.deltaTime;
        if (player == null) FindPlayer();
        if (player == null || throwing) return;

        if (CanSeePlayer())
        {
            SetFacing(player.position.x - transform.position.x);
            if (cooldownTimer <= 0f) StartCoroutine(ThrowRoutine());
        }
    }

    // Busca al jugador por su etiqueta y guarda su collider
    void FindPlayer()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go == null) return;
        player = go.transform;
        playerCollider = go.GetComponent<Collider2D>();
    }

    // Posición de los ojos (cambia de lado según hacia dónde mira)
    Vector2 EyePosition => (Vector2)transform.position + new Vector2(eyeOffset.x * facing, eyeOffset.y);

    // Centro del jugador, que es adonde apunta
    Vector2 PlayerCenter => playerCollider != null ? (Vector2)playerCollider.bounds.center : (Vector2)player.position;

    // Ve al jugador si está cerca, dentro de su cono de visión y sin paredes en medio
    bool CanSeePlayer()
    {
        Vector2 eye = EyePosition;
        Vector2 toPlayer = PlayerCenter - eye;
        float dist = toPlayer.magnitude;

        if (dist > visionRange) return false;
        if (Vector2.Angle(Vector2.right * facing, toPlayer) > visionAngle * 0.5f) return false;

        // Línea de visión: el primer objeto sólido que se cruce debe ser el jugador.
        RaycastHit2D[] hits = Physics2D.RaycastAll(eye, toPlayer.normalized, dist, obstacleLayers);
        foreach (var h in hits)
        {
            if (h.collider.isTrigger) continue;
            if (h.collider.transform.IsChildOf(transform)) continue;       // yo mismo
            if (h.collider.CompareTag(playerTag)) return true;
            if (h.collider.GetComponentInParent<EnemyLevel1>() != null) continue; // otros enemigos no tapan
            return false;                                                  // pared u obstáculo
        }
        return false;
    }

    // Corrutina de lanzar: animación, suelta la botella en el momento justo y espera para el siguiente
    IEnumerator ThrowRoutine()
    {
        throwing = true;
        PlayFrames(throwFrames, throwFps, false, true);

        yield return new WaitForSeconds(throwRelease);
        if (!dead && player != null) ThrowBottle();

        float total = (throwFrames != null && throwFrames.Length > 0) ? throwFrames.Length / throwFps : 0f;
        yield return new WaitForSeconds(Mathf.Max(0f, total - throwRelease));

        cooldownTimer = throwCooldown;
        throwing = false;
        ShowSprite(idleSprite);
    }

    // Crea la botella y le da la velocidad justa para que caiga donde está el jugador
    void ThrowBottle()
    {
        if (bottlePrefab == null) return;

        Vector2 origin = (Vector2)transform.position + new Vector2(throwPoint.x * facing, throwPoint.y);
        Vector2 target = PlayerCenter;

        float gravityScale = 1f;
        var prefabRb = bottlePrefab.GetComponent<Rigidbody2D>();
        if (prefabRb != null) gravityScale = prefabRb.gravityScale;
        float g = Physics2D.gravity.y * gravityScale; // negativo

        // Tiro parabólico que pasa por la posición del jugador en 't' segundos.
        float dx = target.x - origin.x;
        float dy = target.y - origin.y;
        float t = Mathf.Clamp(Mathf.Abs(dx) / Mathf.Max(0.1f, bottleSpeed), 0.35f, 1.2f);
        Vector2 velocity = new Vector2(dx / t, dy / t - 0.5f * g * t);

        var bottle = Instantiate(bottlePrefab, origin, Quaternion.identity);
        if (controladorSonidoLvl1.Instance != null) controladorSonidoLvl1.Instance.ReproducirAtaqueEnemigo();
        if (bottle.TryGetComponent<BottleProjectile>(out var proj))
            proj.Launch(velocity, bottleDamage, facing);
    }

    // Solo en el editor: dibuja el cono de visión en amarillo al seleccionar el enemigo
    void OnDrawGizmosSelected()
    {
        float f = Application.isPlaying ? facing : (startFacingRight ? 1f : -1f);
        Vector2 eye = (Vector2)transform.position + new Vector2(eyeOffset.x * f, eyeOffset.y);

        Gizmos.color = Color.yellow;
        float half = visionAngle * 0.5f * Mathf.Deg2Rad;
        float baseAngle = f > 0 ? 0f : Mathf.PI;
        Vector2 a = new Vector2(Mathf.Cos(baseAngle + half), Mathf.Sin(baseAngle + half)) * visionRange;
        Vector2 b = new Vector2(Mathf.Cos(baseAngle - half), Mathf.Sin(baseAngle - half)) * visionRange;
        Gizmos.DrawLine(eye, eye + a);
        Gizmos.DrawLine(eye, eye + b);
        Gizmos.DrawWireSphere(eye, 0.05f);
    }
}
