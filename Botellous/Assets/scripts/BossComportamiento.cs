using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(BoxCollider2D))]
public class BossComportamiento : MonoBehaviour
{
    // Valores del jefe; se pueden ajustar desde el Inspector.
    [Header("Estadísticas del Jefe")]
    [SerializeField, Min(1f)] private float maxHealth = 300f;
    [SerializeField] private float currentHealth;
    [SerializeField, Min(0.1f)] private float meleeRange = 2.5f;
    [SerializeField, Min(0.1f)] private float movementSpeed = 3.5f;
    [SerializeField] private float minX = 44f;
    [SerializeField] private float maxX = 66f;
    [SerializeField, Min(0.1f)] private float gasAttackRange = 7f;
    [SerializeField, Range(0f, 1f)] private float gasAttackChance = 0.4f;
    [SerializeField, Min(0.1f)] private float gasAttackCooldown = 3.5f;
    [SerializeField, Min(0f)] private float slamDamage = 20f;
    [SerializeField, Min(0f)] private float gasDamage = 15f;

    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject toxicGasZone;

    [Header("Objetos Clave del Jugador")]
    [SerializeField] private bool playerHasSmokeFilter;

    // Identificadores de los parámetros del Animator.
    private static readonly int TriggerSlamHash = Animator.StringToHash("Trigger_Slam");
    private static readonly int TriggerGasHash = Animator.StringToHash("Trigger_Gas");
    private static readonly int TriggerHitHash = Animator.StringToHash("Trigger_Hit");
    private static readonly int DieHash = Animator.StringToHash("Die");
    private static readonly int IsStunnedHash = Animator.StringToHash("IsStunned");
    private static readonly int IsPhase2Hash = Animator.StringToHash("IsPhase2");

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isDead;
    private bool isStunned;
    private bool isPhase2;
    private float nextActionTime;
    private Coroutine stunRoutine;
    private Coroutine gasRoutine;

    // Guarda componentes y comprueba los límites iniciales del jefe.
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (minX >= maxX)
        {
            Debug.LogError("El límite minX del jefe debe ser menor que maxX.", this);
            enabled = false;
            return;
        }

        rb.position = new Vector2(Mathf.Clamp(rb.position.x, minX, maxX), rb.position.y);
        currentHealth = maxHealth;

        if (toxicGasZone == null)
        {
            Debug.LogWarning("No hay una zona visual de gas asignada; el ataque seguirá causando daño dentro de su radio.", this);
        }

        else
        {
            toxicGasZone.SetActive(false);
        }
    }

    // Busca al jugador por su etiqueta si no se asignó desde el Inspector.
    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player == null)
        {
            Debug.LogWarning("BossComportamiento necesita una referencia al jugador o un objeto con la etiqueta Player.", this);
        }
    }

    // Actualiza la orientación, la fase y el momento del siguiente ataque.
    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (player == null)
        {
            StopMovement();
            return;
        }

        if (isStunned)
        {
            return;
        }

        LookAtPlayer();

        if (!isPhase2 && currentHealth <= maxHealth * 0.5f)
        {
            EnterPhase2();
        }

        if (Time.time >= nextActionTime)
        {
            DecideNextAttack();
        }
    }

    // Persigue al jugador sin salir del tramo horizontal permitido.
    private void FixedUpdate()
    {
        if (isDead || isStunned || player == null || IsPlayingActionAnimation())
        {
            StopMovement();
            return;
        }

        float targetX = Mathf.Clamp(player.position.x, minX, maxX);
        float horizontalDistance = targetX - rb.position.x;
        if (Mathf.Abs(horizontalDistance) <= meleeRange)
        {
            StopMovement();
            return;
        }

        float nextX = Mathf.MoveTowards(
            rb.position.x,
            targetX,
            movementSpeed * Time.fixedDeltaTime);
        float horizontalVelocity = (nextX - rb.position.x) / Time.fixedDeltaTime;
        rb.linearVelocity = new Vector2(horizontalVelocity, 0f);
    }

    // Evita que el jefe se desplace mientras ejecuta una acción.
    private bool IsPlayingActionAnimation()
    {
        AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo(0);
        if (IsActionState(currentState))
        {
            return true;
        }

        return anim.IsInTransition(0) && IsActionState(anim.GetNextAnimatorStateInfo(0));
    }

    // Reconoce las animaciones durante las que el jefe debe quedarse quieto.
    private static bool IsActionState(AnimatorStateInfo state)
    {
        return state.IsName("Boss_slam")
            || state.IsName("Boss_gasAttack")
            || state.IsName("Boss_hit")
            || state.IsName("Boss_stun")
            || state.IsName("Boss_death");
    }

    // Detiene al jefe de forma segura.
    private void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Gira el sprite hacia la posición actual del jugador.
    private void LookAtPlayer()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = player.position.x < transform.position.x;
        }
    }

    // Devuelve hacia qué lado debe salir el ataque.
    private float GetFacingDirection()
    {
        return player != null && player.position.x < transform.position.x ? -1f : 1f;
    }

    // Elige un ataque según la distancia y la fase actual del combate.
    private void DecideNextAttack()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= meleeRange)
        {
            anim.SetTrigger(TriggerSlamHash);
            nextActionTime = Time.time + 2f;
            return;
        }

        float currentGasAttackChance = isPhase2
            ? Mathf.Min(gasAttackChance + 0.2f, 1f)
            : gasAttackChance;

        if (distanceToPlayer <= gasAttackRange && Random.value < currentGasAttackChance)
        {
            anim.SetTrigger(TriggerGasHash);
            nextActionTime = Time.time + gasAttackCooldown;
            return;
        }

        nextActionTime = Time.time + 0.4f;
    }

    // Aumenta la agresividad cuando la vida baja a la mitad.
    private void EnterPhase2()
    {
        isPhase2 = true;
        anim.SetBool(IsPhase2Hash, true);
    }

    // Evento llamado por el clip de animación del golpe.
    public void OnAnimationTriggerSlam()
    {
        Vector2 attackCenter = (Vector2)transform.position
            + new Vector2(GetFacingDirection() * 1.5f, -1f);

        DamagePlayerInRange(attackCenter, 1.8f, slamDamage);
    }

    // Evento llamado por el clip de animación del gas.
    public void OnAnimationEmitGas()
    {
        if (playerHasSmokeFilter)
        {
            return;
        }

        DamagePlayerInRange(transform.position, gasAttackRange, gasDamage);

        if (toxicGasZone != null)
        {
            if (gasRoutine != null)
            {
                StopCoroutine(gasRoutine);
            }

            gasRoutine = StartCoroutine(EmitGasRoutine(3.5f));
        }
    }

    public void GivePlayerSmokeFilter()
    {
        playerHasSmokeFilter = true;
    }

    // Busca al jugador dentro del área del ataque y le aplica daño.
    private void DamagePlayerInRange(Vector2 center, float radius, float damage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        foreach (Collider2D hit in hits)
        {
            Botellous.PlayerDamageReceiver playerHealth =
                hit.GetComponentInParent<Botellous.PlayerDamageReceiver>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                return;
            }
        }
    }

    // Mantiene visible el efecto del gas durante un tiempo limitado.
    private IEnumerator EmitGasRoutine(float duration)
    {
        toxicGasZone.SetActive(true);
        yield return new WaitForSeconds(duration);

        if (toxicGasZone != null)
        {
            toxicGasZone.SetActive(false);
        }

        gasRoutine = null;
    }

    // Inicia el aturdimiento del jefe, por ejemplo, al activar una válvula.
    public void ApplyValveStun(float duration = 4f)
    {
        if (isStunned || isDead || duration <= 0f)
        {
            return;
        }

        stunRoutine = StartCoroutine(StunRoutine(duration));
    }

    // Pausa el movimiento y la IA hasta que termine el aturdimiento.
    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        anim.SetBool(IsStunnedHash, true);
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(duration);

        anim.SetBool(IsStunnedHash, false);
        isStunned = false;
        stunRoutine = null;
    }

    // Reduce la vida y activa el impacto, la segunda fase o la derrota.
    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - amount);

        if (!isPhase2 && currentHealth <= maxHealth * 0.5f)
        {
            EnterPhase2();
        }

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        if (!isStunned)
        {
            nextActionTime = Mathf.Max(nextActionTime, Time.time + 0.7f);
            anim.SetTrigger(TriggerHitHash);
        }
    }

    // Termina el combate y limpia las acciones que sigan activas.
    private void Die()
    {
        isDead = true;
        anim.SetTrigger(DieHash);
        rb.linearVelocity = Vector2.zero;

        Collider2D bossCollider = GetComponent<Collider2D>();
        if (bossCollider != null)
        {
            bossCollider.enabled = false;
        }

        if (stunRoutine != null)
        {
            StopCoroutine(stunRoutine);
            stunRoutine = null;
        }

        isStunned = false;
        anim.SetBool(IsStunnedHash, false);

        if (gasRoutine != null)
        {
            StopCoroutine(gasRoutine);
            gasRoutine = null;
        }

        if (toxicGasZone != null)
        {
            toxicGasZone.SetActive(false);
        }
    }

    // Detiene las rutinas y oculta el gas cuando se desactiva el componente.
    private void OnDisable()
    {
        if (stunRoutine != null)
        {
            StopCoroutine(stunRoutine);
            stunRoutine = null;
        }

        isStunned = false;
        if (anim != null)
        {
            anim.SetBool(IsStunnedHash, false);
        }

        if (gasRoutine != null)
        {
            StopCoroutine(gasRoutine);
            gasRoutine = null;
        }

        if (toxicGasZone != null)
        {
            toxicGasZone.SetActive(false);
        }
    }
}
