using System.Collections;
using UnityEngine;

/// <summary>
/// Botella lanzada por Enemy1L2. Gira en el aire, hace daño al jugador al impactar
/// y se rompe (animación de cristales) al chocar con cualquier cosa sólida.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BottleProjectile : MonoBehaviour
{
    public float damage = 15f;
    public float spinSpeed = 540f;
    public float lifeTime = 6f;
    public string targetTag = "Player";

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] shatterFrames;
    public float shatterFps = 8f;
    public float shatterLinger = 0.8f;

    Rigidbody2D rb;
    float spinDir = -1f;
    bool broken;

    // Coge el Rigidbody y el sprite de la botella
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // La llama Enemy1L2 al lanzarla: velocidad inicial, daño y hacia qué lado gira
    public void Launch(Vector2 velocity, float newDamage, float throwDirection)
    {
        damage = newDamage;
        spinDir = -Mathf.Sign(throwDirection);
        rb.linearVelocity = velocity;
    }

    // Gira la botella en el aire y la destruye si pasa demasiado tiempo sin chocar
    void Update()
    {
        if (broken) return;

        spriteRenderer.transform.Rotate(0f, 0f, spinDir * spinSpeed * Time.deltaTime);

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f) Destroy(gameObject);
    }

    // Al chocar: si es el jugador le hace daño; con cualquier cosa sólida se rompe
    void OnTriggerEnter2D(Collider2D other)
    {
        if (broken || other.isTrigger) return;
        if (other.GetComponentInParent<EnemyLevel1>() != null) return; // no choca con enemigos

        if (other.CompareTag(targetTag))
        {
            var target = other.GetComponentInParent<IDamageable>();
            if (target != null) target.TakeDamage(damage);
        }

        StartCoroutine(Shatter());
    }

    // Corrutina de romperse: para la botella, suena el cristal, enseña los trozos y la borra
    IEnumerator Shatter()
    {
        broken = true;
        if (controladorSonidoLvl1.Instance != null) controladorSonidoLvl1.Instance.ReproducirBotellaRota();
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        spriteRenderer.transform.rotation = Quaternion.identity;

        if (shatterFrames != null && shatterFrames.Length > 0)
        {
            float step = 1f / Mathf.Max(1f, shatterFps);
            foreach (var s in shatterFrames)
            {
                spriteRenderer.sprite = s;
                yield return new WaitForSeconds(step);
            }
        }

        yield return new WaitForSeconds(shatterLinger);
        Destroy(gameObject);
    }
}
