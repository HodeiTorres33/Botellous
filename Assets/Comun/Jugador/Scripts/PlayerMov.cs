using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private int damageNivel2 = 1;
    [SerializeField] private float danoPorDaga = 5f;

    public static float danoExtra = 0f;
    private float danoExtraAlEmpezar;

    // Nombres de los parámetros usados por el Animator del jugador.
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D bodyCollider;
    private SpriteRenderer spriteRenderer;
    private readonly HashSet<GameObject> enemiesHitThisAttack = new HashSet<GameObject>();
    private float horizontalInput;
    private float nextAttackTime;
    private bool isGrounded = true;
    private bool jumpRequested;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void EmpezarPartida()
    {
        danoExtra = 0f;
    }

    private void Start()
    {
        danoExtraAlEmpezar = danoExtra;
        attackDamage = attackDamage + danoExtra;
    }

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
    }

    // Lee los controles y actualiza la orientación y las animaciones.
    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        horizontalInput = 0f;

        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed) horizontalInput -= 1f;
            if (keyboard.dKey.isPressed) horizontalInput += 1f;

            if (keyboard.wKey.wasPressedThisFrame && isGrounded)
            {
                jumpRequested = true;
            }
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame
            && isGrounded && Time.time >= nextAttackTime
            && !ItemPickup.IsPointerOverPrompt(mouse.position.ReadValue()))
        {
            enemiesHitThisAttack.Clear();
            anim.SetTrigger(AttackHash);
            nextAttackTime = Time.time + attackCooldown;
        }

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
        Vector2 feet = new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.min.y);
        Vector2 feetSize = new Vector2(bodyCollider.bounds.size.x * 0.9f, groundCheckDistance);
        isGrounded = false;
        foreach (Collider2D ground in Physics2D.OverlapBoxAll(feet, feetSize, 0f, groundLayers))
        {
            if (ground != bodyCollider && !ground.isTrigger)
            {
                isGrounded = true;
            }
        }

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

    public void AddDamage(float amount)
    {
        attackDamage += amount;
        danoExtra = danoExtra + amount;
    }

    // Evento del clip de ataque: detecta al jefe y le aplica daño una vez por golpe.
    public void OnAnimationAttackHit()
    {
        float facingDirection = spriteRenderer != null && spriteRenderer.flipX ? -1f : 1f;
        Vector2 attackCenter = new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.center.y)
            + Vector2.right * facingDirection * (attackRange * 0.65f);
        Vector2 attackSize = new Vector2(attackRange, attackHeight);

        foreach (Collider2D hit in Physics2D.OverlapBoxAll(attackCenter, attackSize, 0f))
        {
            BossComportamiento boss = hit.GetComponentInParent<BossComportamiento>();
            if (boss != null && enemiesHitThisAttack.Add(boss.gameObject))
            {
                boss.TakeDamage(attackDamage);
            }

            EnemyLevel1 enemyNivel1 = hit.GetComponentInParent<EnemyLevel1>();
            if (enemyNivel1 != null && enemiesHitThisAttack.Add(enemyNivel1.gameObject))
            {
                enemyNivel1.TakeDamage(attackDamage);
            }

            Enemigo enemigoNivel2 = hit.GetComponentInParent<Enemigo>();
            if (enemigoNivel2 != null && enemiesHitThisAttack.Add(enemigoNivel2.gameObject))
            {
                enemigoNivel2.RecibirDano(damageNivel2 + Mathf.FloorToInt(danoExtra / danoPorDaga));
            }
        }
    }
}
