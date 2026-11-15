using System.Globalization;
using UnityEngine;

/// <summary>
/// Número de daño flotante. Se crea por código con DamagePopup.Show(...):
/// no necesita prefabs, fuentes ni Canvas. Sube y se desvanece.
/// </summary>
public class DamagePopup : MonoBehaviour
{
    public float riseSpeed = 1.2f;
    public float lifeTime = 0.8f;

    static Font font;

    TextMesh[] texts;
    Color[] baseColors;
    float timer;

    // Crea un número de daño en 'position' (un poco desplazado a un lado para que no se tapen)
    public static void Show(Vector3 position, float amount, Color color)
    {
        position.x += Random.Range(-0.15f, 0.15f);
        position.z = 0f;

        var go = new GameObject("DamagePopup");
        go.transform.position = position;
        var popup = go.AddComponent<DamagePopup>();
        popup.Build(amount.ToString("0.#", CultureInfo.InvariantCulture), color);
    }

    // Monta el texto: una sombra negra detrás y el número de color delante
    void Build(string label, Color color)
    {
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 32);
        }

        var shadow = Make("Sombra", label, new Color(0f, 0f, 0f, 0.9f), 199, new Vector3(0.025f, -0.025f, 0f));
        var main   = Make("Texto",  label, color, 200, Vector3.zero);

        texts = new[] { main, shadow };
        baseColors = new[] { color, shadow.color };
        transform.localScale = Vector3.one * 1.3f;
    }

    // Crea un objeto hijo con un TextMesh configurado (fuente, tamaño, color y orden de dibujo)
    TextMesh Make(string childName, string label, Color color, int order, Vector3 localPos)
    {
        var child = new GameObject(childName);
        child.transform.SetParent(transform, false);
        child.transform.localPosition = localPos;

        var tm = child.AddComponent<TextMesh>();
        tm.font = font;
        tm.text = label;
        tm.fontSize = 64;
        tm.characterSize = 0.05f;
        tm.fontStyle = FontStyle.Bold;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color;

        var mr = child.GetComponent<MeshRenderer>();
        mr.sharedMaterial = font.material;
        mr.sortingOrder = order;
        return tm;
    }

    // Sube poco a poco, vuelve a su tamaño normal y se borra al acabar su tiempo
    void Update()
    {
        timer += Time.deltaTime;
        transform.position += Vector3.up * (riseSpeed * Time.deltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * 12f);

        // Se desvanece durante la segunda mitad de su vida.
        float fade = Mathf.Clamp01(1f - (timer / lifeTime - 0.5f) * 2f);
        for (int i = 0; i < texts.Length; i++)
        {
            var c = baseColors[i];
            c.a *= fade;
            texts[i].color = c;
        }

        if (timer >= lifeTime) Destroy(gameObject);
    }
}
