using UnityEngine;

/// <summary>
/// Enemigo con navaja: camina de un lado a otro y cambia de dirección
/// cuando choca contra algo en horizontal (pared, objeto, jugador...).
/// </summary>
public class Enemy1L1 : EnemyLevel1
{
    [Header("Patrulla")]
    public float moveSpeed = 2f;
    public bool startFacingRight = true;

    [Header("Animación")]
    public Sprite[] walkFrames;
    public float walkFps = 10f;

    float dir;
    float lastFlipTime = -1f;

    protected override void Awake()
    {
        base.Awake();
        dir = startFacingRight ? 1f : -1f;
    }

    void Start()
    {
        SetFacing(dir);
        PlayFrames(walkFrames, walkFps);
    }

    void FixedUpdate()
    {
        if (dead) return;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    protected override void OnCollisionEnter2D(Collision2D c)
    {
        base.OnCollisionEnter2D(c);
        if (dead) return;

        for (int i = 0; i < c.contactCount; i++)
        {
            ContactPoint2D p = c.GetContact(i);
            if (Mathf.Abs(p.normal.x) < 0.5f) continue;                       // suelo/techo: no cuenta
            if (Mathf.Sign(p.point.x - transform.position.x) != dir) continue; // choque por detrás: no cuenta

            Flip();
            break;
        }
    }

    void Flip()
    {
        if (Time.time - lastFlipTime < 0.1f) return;
        lastFlipTime = Time.time;
        dir = -dir;
        SetFacing(dir);
    }
}
