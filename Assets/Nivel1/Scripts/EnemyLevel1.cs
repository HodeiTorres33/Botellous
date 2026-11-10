using System.Collections;
using UnityEngine;

/// <summary>
/// Clase PADRE de los enemigos del nivel 1.
/// Contiene los atributos base (vida, daño por contacto, drops), la interfaz de vida
/// (IDamageable), el daño al tocar al jugador y un pequeño animador de sprites.
/// Los hijos (Enemy1L1, Enemy1L2) solo añaden su comportamiento propio.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public abstract class EnemyLevel1 : MonoBehaviour, IDamageable
{
    [Header("Estadísticas base")]
    public float maxHealth = 30f;
    public float currentHealth = 30f;
    [Tooltip("Daño que hace al jugador al tocarlo.")]
    public float contactDamage = 10f;
    [Tooltip("Segundos entre golpes por contacto.")]
    public float contactDamageCooldown = 1f;

    [Header("Drops (items que suelta al morir)")]
    public GameObject[] possibleDrops;
    [Range(0f, 1f)] public float dropChance = 0.5f;
    public float vidaAlMorir = 20f;

    [Header("Referencias")]
    public SpriteRenderer spriteRenderer;
    public string playerTag = "Player";

    public event System.Action<float, float> OnHealthChanged; // (actual, máxima)
    public event System.Action<EnemyLevel1> OnDied;

    protected Rigidbody2D rb;
    protected BoxCollider2D box;
    protected float facing = 1f;   // 1 = derecha, -1 = izquierda
    protected bool dead;

    static readonly Color DamageTextColor = new Color(1f, 0.9f, 0.3f);

    float nextContactDamageTime;
    float visualScaleX = 1f;
    Coroutine flashRoutine;

    // Animación de sprites
    Sprite[] frames;
    float frameDuration, frameTimer;
    int frameIndex;
    bool frameLoop;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        box.sharedMaterial = new PhysicsMaterial2D("EnemigoSinFriccion") { friction = 0f, bounciness = 0f };

        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) visualScaleX = Mathf.Abs(spriteRenderer.transform.localScale.x);

        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        if (dead) return;
        TickFrames();
    }

    // ---------------- VIDA ----------------
    public virtual void TakeDamage(float amount)
    {
        if (dead || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Número de daño flotante sobre el enemigo
        DamagePopup.Show(transform.position + Vector3.up * (box.size.y * 0.5f + 0.25f), amount, DamageTextColor);

        if (currentHealth <= 0f) { Die(); return; }
        Flash();
    }

    public void Heal(float amount)
    {
        if (dead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    protected virtual void Die()
    {
        dead = true;
        OnDied?.Invoke(this);
        DropLoot();

        var jugador = GameObject.FindGameObjectWithTag(playerTag);
        if (jugador != null)
            jugador.SendMessage("Heal", vidaAlMorir, SendMessageOptions.DontRequireReceiver);
        Destroy(gameObject);
    }

    void DropLoot()
    {
        if (possibleDrops == null || possibleDrops.Length == 0) return;
        if (Random.value > dropChance) return;

        var drop = possibleDrops[Random.Range(0, possibleDrops.Length)];
        if (drop != null) Instantiate(drop, transform.position, Quaternion.identity);
    }

    // ---------------- DAÑO POR CONTACTO ----------------
    protected virtual void OnCollisionEnter2D(Collision2D c) { TryHitPlayer(c.collider); }
    void OnCollisionStay2D(Collision2D c) { TryHitPlayer(c.collider); }

    void TryHitPlayer(Collider2D other)
    {
        if (dead || Time.time < nextContactDamageTime) return;
        if (!other.CompareTag(playerTag)) return;

        var target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;

        target.TakeDamage(contactDamage);
        if (controladorSonidoLvl1.Instance != null) controladorSonidoLvl1.Instance.ReproducirAtaqueEnemigo();
        nextContactDamageTime = Time.time + contactDamageCooldown;
    }

    // ---------------- UTILIDADES PARA LOS HIJOS ----------------
    /// <summary>Gira el sprite hacia la dirección indicada (+ derecha, - izquierda).</summary>
    protected void SetFacing(float dir)
    {
        if (dir == 0f) return;
        facing = Mathf.Sign(dir);
        if (spriteRenderer == null) return;

        var t = spriteRenderer.transform;
        var s = t.localScale;
        s.x = visualScaleX * facing;
        t.localScale = s;
    }

    /// <summary>Reproduce una secuencia de sprites.</summary>
    protected void PlayFrames(Sprite[] sequence, float fps, bool loop = true, bool restart = false)
    {
        if (sequence == null || sequence.Length == 0 || spriteRenderer == null) return;
        if (frames == sequence && !restart) return;

        frames = sequence;
        frameDuration = 1f / Mathf.Max(1f, fps);
        frameTimer = 0f;
        frameIndex = 0;
        frameLoop = loop;
        spriteRenderer.sprite = frames[0];
    }

    /// <summary>Muestra un sprite fijo (detiene la animación).</summary>
    protected void ShowSprite(Sprite sprite)
    {
        frames = null;
        if (spriteRenderer != null && sprite != null) spriteRenderer.sprite = sprite;
    }

    void TickFrames()
    {
        if (frames == null || spriteRenderer == null) return;

        frameTimer += Time.deltaTime;
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex++;
            if (frameIndex >= frames.Length)
            {
                if (!frameLoop) { frameIndex = frames.Length - 1; frameTimer = 0f; break; }
                frameIndex = 0;
            }
            spriteRenderer.sprite = frames[frameIndex];
        }
    }

    void Flash()
    {
        if (spriteRenderer == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 1f);
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
        flashRoutine = null;
    }
}
