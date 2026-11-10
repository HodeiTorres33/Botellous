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

    // Nombres de los parámetros usados por el Animator del jugador.
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D bodyCollider;
    private SpriteRenderer spriteRenderer;
    private readonly HashSet<BossComportamiento> bossesHitThisAttack = new HashSet<BossComportamiento>();
    private float horizontalInput;
    private float nextAttackTime;
    private bool isGrounded = true;
    private bool jumpRequested;

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
            && isGrounded && Time.time >= nextAttackTime)
        {
            bossesHitThisAttack.Clear();
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollectGasMask(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryCollectGasMask(collision.otherCollider);
    }

    // Notifica al jefe cuando el jugador entra en contacto con la máscara.
    private void TryCollectGasMask(Collider2D other)
    {
        for (Transform current = other.transform; current != null; current = current.parent)
        {
            if (current.name != "mascaraGas")
            {
                continue;
            }

            BossComportamiento boss = FindAnyObjectByType<BossComportamiento>(FindObjectsInactive.Include);
            if (boss == null)
            {
                Debug.LogError("No se encontró un BossComportamiento para registrar la máscara de gas.", this);
                return;
            }

            boss.GivePlayerSmokeFilter();
            return;
        }
    }

    // Aplica el movimiento y comprueba el contacto con el suelo en cada paso físico.
    private void FixedUpdate()
    {
        isGrounded = Physics2D.Raycast(
            new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.min.y - 0.02f),
            Vector2.down,
            groundCheckDistance,
            groundLayers);

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
            if (boss != null && bossesHitThisAttack.Add(boss))
            {
                boss.TakeDamage(attackDamage);
            }
        }
    }
}
