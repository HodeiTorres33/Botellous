using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Control de un personaje 2D de plataformas (cuadrado).
///   Moverse:  A / D  o  flechas izquierda / derecha
///   Saltar:   Espacio, W o flecha arriba (mantén para saltar más alto)
///   Atacar:   Click izquierdo
///   Recoger:  E (o tocar el botón E que aparece sobre el item)
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Movimiento")]
    public float moveSpeed = 6f;
    public float jumpSpeed = 12f;

    [Header("Salto")]
    [Tooltip("Tiempo extra para saltar justo después de dejar una plataforma.")]
    public float coyoteTime = 0.1f;
    [Tooltip("Tiempo que se recuerda un salto pulsado justo antes de aterrizar.")]
    public float jumpBufferTime = 0.1f;
    [Tooltip("Multiplicador de gravedad al soltar el salto (salto variable).")]
    public float lowJumpMultiplier = 2.5f;

    [Header("Suelo")]
    public LayerMask groundLayers = ~0;
    public float groundCheckDistance = 0.05f;

    [Header("Estadísticas (modificables por items)")]
    public float damage = 10f;
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Ataque")]
    [Tooltip("Capas que pueden recibir daño (pon aquí la capa de los enemigos).")]
    public LayerMask enemyLayers = ~0;
    [Tooltip("Distancia desde el centro del jugador hasta el centro del golpe.")]
    public float attackOffset = 0.8f;
    public float attackRadius = 0.6f;
    [Tooltip("Segundos entre ataques.")]
    public float attackCooldown = 0.4f;

    [Header("Feedback de daño")]
    public Color hurtColor = new Color(1f, 0.15f, 0.15f, 1f);
    public float hurtFlashTime = 0.15f;
    public Color damageTextColor = new Color(1f, 0.25f, 0.25f, 1f);

    // Eventos útiles para una barra de vida / HUD
    public event System.Action<float, float> OnHealthChanged; // (actual, máxima)
    public event System.Action OnDied;

    Rigidbody2D rb;
    BoxCollider2D box;
    SpriteRenderer sprite;
    Color spriteBaseColor = Color.white;
    Coroutine hurtRoutine;

    float moveInput;
    bool jumpPressed, jumpHeld, attackPressed;
    float coyoteCounter, bufferCounter, attackTimer;
    bool grounded;
    float baseGravity;
    float facing = 1f;
    bool dead;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        baseGravity = rb.gravityScale;

        sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null) spriteBaseColor = sprite.color;

        // Sin fricción para que no se quede pegado a las paredes.
        box.sharedMaterial = new PhysicsMaterial2D("SinFriccion") { friction = 0f, bounciness = 0f };

        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    void Update()
    {
        if (dead) return;

        ReadInput();

        if (moveInput != 0f) facing = Mathf.Sign(moveInput);

        if (jumpPressed) bufferCounter = jumpBufferTime;
        else bufferCounter -= Time.deltaTime;

        if (grounded) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.deltaTime;

        attackTimer -= Time.deltaTime;
        if (attackPressed && attackTimer <= 0f)
        {
            Attack();
            attackTimer = attackCooldown;
        }
    }

    void FixedUpdate()
    {
        if (dead) return;

        grounded = CheckGrounded();

        Vector2 v = rb.linearVelocity;
        v.x = moveInput * moveSpeed;

        if (bufferCounter > 0f && coyoteCounter > 0f)
        {
            v.y = jumpSpeed;
            bufferCounter = 0f;
            coyoteCounter = 0f;
        }

        // Si suelta el botón mientras sube, cae antes: salto más corto.
        rb.gravityScale = (v.y > 0f && !jumpHeld) ? baseGravity * lowJumpMultiplier : baseGravity;

        rb.linearVelocity = v;
    }

    bool CheckGrounded()
    {
        Bounds b = box.bounds;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(b.center, new Vector2(b.size.x * 0.95f, b.size.y),
                                                   0f, Vector2.down, groundCheckDistance, groundLayers);
        foreach (var h in hits)
        {
            if (h.collider == box || h.collider.isTrigger) continue;
            if (h.normal.y > 0.5f) return true;   // solo cuenta si pisa por arriba
        }
        return false;
    }

    // ---------------- COMBATE ----------------
    void Attack()
    {
        Vector2 center = (Vector2)transform.position + Vector2.right * facing * attackOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRadius, enemyLayers);

        // Un enemigo con varios colliders solo recibe el golpe una vez.
        // (El número de daño lo muestra el propio enemigo al recibirlo.)
        var alreadyHit = new HashSet<IDamageable>();
        foreach (var h in hits)
        {
            if (h.gameObject == gameObject) continue;
            if (h.TryGetComponent<IDamageable>(out var target) && alreadyHit.Add(target))
                target.TakeDamage(damage);
        }
    }

    public void TakeDamage(float amount)
    {
        if (dead || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Número de daño sobre la cabeza + parpadeo rojo
        DamagePopup.Show(new Vector3(transform.position.x, box.bounds.max.y + 0.25f, 0f), amount, damageTextColor);
        FlashHurt();

        if (currentHealth <= 0f) Die();
    }

    void FlashHurt()
    {
        if (sprite == null) return;
        if (hurtRoutine != null) StopCoroutine(hurtRoutine);
        hurtRoutine = StartCoroutine(HurtRoutine());
    }

    IEnumerator HurtRoutine()
    {
        sprite.color = hurtColor;
        yield return new WaitForSeconds(hurtFlashTime);
        sprite.color = spriteBaseColor;
        hurtRoutine = null;
    }

    void Die()
    {
        dead = true;
        rb.linearVelocity = Vector2.zero;
        OnDied?.Invoke();
        Debug.Log("El jugador ha muerto");
        // Aquí puedes recargar escena, mostrar pantalla de Game Over, etc.
    }

    // ---------------- API PARA ITEMS ----------------
    /// <summary>Suma (o resta, si es negativo) daño base.</summary>
    public void AddDamage(float amount) => damage += amount;

    /// <summary>Aumenta la vida máxima. Opcionalmente cura esa misma cantidad.</summary>
    public void AddMaxHealth(float amount, bool alsoHeal = true)
    {
        maxHealth += amount;
        if (alsoHeal) currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Heal(float amount)
    {
        if (dead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Para ver la zona de golpe en el editor (selecciona al jugador).
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.right * facing * attackOffset, attackRadius);
    }

    // ---------------- INPUTS ----------------
    void ReadInput()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        var mouse = Mouse.current;

        attackPressed = mouse != null && mouse.leftButton.wasPressedThisFrame;
        // Pulsar/tocar el botón E de un item no cuenta como ataque.
        if (attackPressed && ItemPickup.IsPointerOverPrompt(PointerPosition())) attackPressed = false;

        if (kb == null) { moveInput = 0f; jumpPressed = jumpHeld = false; return; }

        moveInput = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  moveInput -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveInput += 1f;

        jumpPressed = kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame;
        jumpHeld    = kb.spaceKey.isPressed || kb.wKey.isPressed || kb.upArrowKey.isPressed;
#else
        attackPressed = Input.GetMouseButtonDown(0);
        if (attackPressed && ItemPickup.IsPointerOverPrompt(PointerPosition())) attackPressed = false;

        moveInput = Input.GetAxisRaw("Horizontal");
        jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
#endif
    }

    static Vector2 PointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.position.ReadValue();
        if (Touchscreen.current != null) return Touchscreen.current.primaryTouch.position.ReadValue();
        return Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }
}