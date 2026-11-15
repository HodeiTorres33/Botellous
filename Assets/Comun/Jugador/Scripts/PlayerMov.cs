using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Movimiento y ataque del jugador. Es el mismo en los 3 niveles.
// Teclado: A/D andar, W saltar, clic izquierdo atacar.
// En Android los botones táctiles (ControlesMovil) imitan esas mismas teclas.
[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(BoxCollider2D))]
public class PlayerMov : MonoBehaviour
{
    // Parámetros ajustables de movimiento, suelo y ataque.
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundCheckDistance = 0.12f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackHeight = 2.6f;
    [SerializeField] private float attackCooldown = 0.7f;
    // Los esqueletos del nivel 2 tienen la vida en puntos, así que les quitamos puntos y no vida
    [SerializeField] private int damageNivel2 = 1;
    // Daño que suma cada daga (DamageItem); sirve para saber cuántas dagas lleva el jugador
    [SerializeField] private float danoPorDaga = 5f;
    // Sonido del espadazo. Solo está puesto en el nivel 2; si se deja vacío no suena nada.
    [SerializeField] private AudioClip sonidoAtaque;
    [SerializeField, Range(0f, 1f)] private float volumenAtaque = 0.7f;

    // Daño extra de las dagas. Es "static" para que no se borre al cambiar de escena
    // y así las mejoras del nivel 1 sigan en los niveles 2 y 3.
    public static float danoExtra = 0f;
    // Daño extra que tenía al empezar este nivel (para devolverlo si muere y no duplicar dagas)
    private float danoExtraAlEmpezar;

    // Nombres de los parámetros usados por el Animator del jugador.
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D bodyCollider;
    private SpriteRenderer spriteRenderer;
    // Altavoz para los efectos del jugador (se crea solo si hay algún sonido asignado)
    private AudioSource efectos;
    // Enemigos ya golpeados en el ataque actual, para no dañar dos veces al mismo con un golpe
    private readonly HashSet<GameObject> enemiesHitThisAttack = new HashSet<GameObject>();
    private float horizontalInput;
    private float nextAttackTime;
    private bool isGrounded = true;
    private bool jumpRequested;

    // Se ejecuta cada vez que empieza el juego (también al darle a Play en el editor).
    // Hace falta porque el proyecto no recarga los scripts al darle a Play,
    // y sin esto las dagas de una prueba se sumarían a la siguiente.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void EmpezarPartida()
    {
        danoExtra = 0f;
    }

    // Al empezar el nivel suma al daño las dagas que trae de los niveles anteriores.
    private void Start()
    {
        danoExtraAlEmpezar = danoExtra;
        attackDamage = attackDamage + danoExtra;
    }

    // Se llama al morir o al reiniciar el nivel: quita las dagas cogidas en este intento,
    // porque los enemigos las vuelven a soltar al reiniciar.
    public void QuitarMejorasDelNivel()
    {
        danoExtra = danoExtraAlEmpezar;
    }

    // Obtiene una vez los componentes que necesita el jugador.
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        bodyCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (sonidoAtaque != null)
        {
            efectos = gameObject.AddComponent<AudioSource>();
            efectos.playOnAwake = false;
            // 0 = sonido 2D: se oye igual esté donde esté la cámara
            efectos.spatialBlend = 0f;
        }
    }

    // Lee los controles y actualiza la orientación y las animaciones.
    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        horizontalInput = 0f;

        if (keyboard != null)
        {
            // A = izquierda (-1), D = derecha (+1); si se pulsan las dos se queda quieto
            if (keyboard.aKey.isPressed) horizontalInput -= 1f;
            if (keyboard.dKey.isPressed) horizontalInput += 1f;

            // Solo se puede saltar si está tocando el suelo
            if (keyboard.wKey.wasPressedThisFrame && isGrounded)
            {
                jumpRequested = true;
            }
        }

        // Ataque: en el suelo, cuando ha pasado el tiempo de espera,
        // y sin estar pulsando la "E" de un objeto del nivel 1
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame
            && isGrounded && Time.time >= nextAttackTime
            && !ItemPickup.IsPointerOverPrompt(mouse.position.ReadValue()))
        {
            enemiesHitThisAttack.Clear();
            // La animación de ataque llama a OnAnimationAttackHit en el momento del golpe
            anim.SetTrigger(AttackHash);
            nextAttackTime = Time.time + attackCooldown;

            if (efectos != null)
            {
                efectos.PlayOneShot(sonidoAtaque, volumenAtaque);
            }
        }

        // Gira el sprite hacia donde anda
        if (spriteRenderer != null && horizontalInput != 0f)
        {
            spriteRenderer.flipX = horizontalInput < 0f;
        }

        anim.SetBool(IsMovingHash, horizontalInput != 0f);
        anim.SetBool(IsGroundedHash, isGrounded);
    }

    // Aplica el movimiento y comprueba el contacto con el suelo en cada paso físico.
    private void FixedUpdate()
    {
        // Caja fina justo debajo de los pies: si toca algo sólido, está en el suelo.
        // (Con un rayo no se detectaban las plataformas del nivel 1.)
        Vector2 feet = new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.min.y);
        Vector2 feetSize = new Vector2(bodyCollider.bounds.size.x * 0.9f, groundCheckDistance);
        isGrounded = false;
        foreach (Collider2D ground in Physics2D.OverlapBoxAll(feet, feetSize, 0f, groundLayers))
        {
            // No cuentan ni el propio jugador ni las zonas trigger
            if (ground != bodyCollider && !ground.isTrigger)
            {
                isGrounded = true;
            }
        }

        // La velocidad horizontal la ponemos nosotros; la vertical la deja la gravedad
        Vector2 velocity = rb.linearVelocity;
        velocity.x = horizontalInput * movementSpeed;

        if (jumpRequested && isGrounded)
        {
            velocity.y = jumpForce;
            isGrounded = false;
        }

        rb.linearVelocity = velocity;
        jumpRequested = false;
    }

    // true si el jugador ya ha cogido una daga (en este nivel o en uno anterior)
    public static bool TieneDaga
    {
        get { return danoExtra > 0f; }
    }

    // La daga del nivel 1 (DamageItem) sube el daño y lo guarda para los siguientes niveles.
    // El bufo no se acumula: solo cuenta la primera daga; las demás no hacen nada.
    public void AddDamage(float amount)
    {
        if (TieneDaga)
        {
            return;
        }

        attackDamage += amount;
        danoExtra = amount;
    }

    // Evento del clip de ataque: daña una vez por golpe a lo que haya delante del jugador:
    // el jefe (nivel 3), los enemigos del nivel 1 y los esqueletos del nivel 2.
    public void OnAnimationAttackHit()
    {
        // Caja de ataque delante del jugador, hacia el lado al que mira
        float facingDirection = spriteRenderer != null && spriteRenderer.flipX ? -1f : 1f;
        Vector2 attackCenter = new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.center.y)
            + Vector2.right * facingDirection * (attackRange * 0.65f);
        Vector2 attackSize = new Vector2(attackRange, attackHeight);

        foreach (Collider2D hit in Physics2D.OverlapBoxAll(attackCenter, attackSize, 0f))
        {
            // Jefe del nivel 3
            BossComportamiento boss = hit.GetComponentInParent<BossComportamiento>();
            if (boss != null && enemiesHitThisAttack.Add(boss.gameObject))
            {
                boss.TakeDamage(attackDamage);
            }

            // Enemigos del nivel 1 (vida con decimales)
            EnemyLevel1 enemyNivel1 = hit.GetComponentInParent<EnemyLevel1>();
            if (enemyNivel1 != null && enemiesHitThisAttack.Add(enemyNivel1.gameObject))
            {
                enemyNivel1.TakeDamage(attackDamage);
            }

            // Esqueletos del nivel 2 (vida en puntos): 1 punto + 1 más por cada daga
            Enemigo enemigoNivel2 = hit.GetComponentInParent<Enemigo>();
            if (enemigoNivel2 != null && enemiesHitThisAttack.Add(enemigoNivel2.gameObject))
            {
                enemigoNivel2.RecibirDano(damageNivel2 + Mathf.FloorToInt(danoExtra / danoPorDaga));
            }
        }
    }
}
