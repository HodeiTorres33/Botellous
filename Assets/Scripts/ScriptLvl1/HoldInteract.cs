
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Interacción manteniendo pulsada la tecla E.
/// Muestra un círculo con la tecla encima del objeto.
/// </summary>
public class HoldInteract : MonoBehaviour
{
    [Header("Interacción")]
    [Tooltip("Segundos que hay que mantener pulsado.")]
    public float holdTime = 0.8f;

    public string playerTag = "Player";

    [Header("Aspecto")]
    [Tooltip("Altura del indicador sobre este objeto.")]
    public float promptHeight = 0.8f;

    [Range(0.04f, 0.6f)]
    public float promptSize = 0.108f;

    public Color fillColor = new Color(1f, 0.85f, 0.3f, 1f);

    public event System.Action<PlayerController> OnInteracted;

    public float Progress01 => progress;
    public bool PlayerInRange => player != null;

    PlayerController player;
    float progress;
    bool waitRelease;

    Rect promptRect;
    Rect hitRect;

    GameObject viewRoot;
    RectTransform rootRt, circleRt, ringRt, fillRt, letterRt;
    Image fillImage;
    Text letterText;

    static Canvas canvas;
    static Sprite discSprite, ringSprite;
    static Font uiFont;
    static readonly List<HoldInteract> all = new List<HoldInteract>();

    void OnEnable()
    {
        all.Add(this);
    }

    void OnDisable()
    {
        all.Remove(this);

        if (viewRoot != null)
            viewRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (viewRoot != null)
            Destroy(viewRoot);
    }

    void Update()
    {
        if (player == null)
        {
            progress = 0f;

            if (viewRoot != null && viewRoot.activeSelf)
                viewRoot.SetActive(false);

            return;
        }

        UpdatePromptRect();

        bool pressing = KeyHeld() || PointerHeld();

        if (waitRelease)
        {
            if (!pressing)
                waitRelease = false;

            pressing = false;
        }

        float step = Time.deltaTime / Mathf.Max(0.05f, holdTime);

        if (pressing)
        {
            progress += step;

            if (progress >= 1f)
            {
                progress = 0f;
                waitRelease = true;

                OnInteracted?.Invoke(player);

                if (this == null)
                    return;
            }
        }
        else
        {
            progress = Mathf.MoveTowards(progress, 0f, step * 3f);
        }

        UpdateView();
    }

    // DETECCIÓN DEL JUGADOR

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        var p = other.GetComponentInParent<PlayerController>();

        if (p == null)
            return;

        player = p;
        progress = 0f;
        waitRelease = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (player != null &&
            other.GetComponentInParent<PlayerController>() == player)
        {
            player = null;
            progress = 0f;

            if (viewRoot != null)
                viewRoot.SetActive(false);
        }
    }

    // INPUT

    static bool KeyHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
               Keyboard.current.eKey.isPressed;
#else
        return Input.GetKey(KeyCode.E);
#endif
    }

    bool PointerHeld()
    {
#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;

        if (mouse != null &&
            mouse.leftButton.isPressed &&
            HitTest(mouse.position.ReadValue()))
        {
            return true;
        }

        var ts = Touchscreen.current;

        if (ts != null)
        {
            foreach (var t in ts.touches)
            {
                if (t.press.isPressed &&
                    HitTest(t.position.ReadValue()))
                {
                    return true;
                }
            }
        }

        return false;
#else
        if (Input.GetMouseButton(0) && HitTest(Input.mousePosition))
            return true;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            if (t.phase != TouchPhase.Ended &&
                t.phase != TouchPhase.Canceled &&
                HitTest(t.position))
            {
                return true;
            }
        }

        return false;
#endif
    }

    bool HitTest(Vector2 screenPos)
    {
        return hitRect.Contains(
            new Vector2(
                screenPos.x,
                Screen.height - screenPos.y
            )
        );
    }

    public static bool IsPointerOverPrompt(Vector2 screenPos)
    {
        Vector2 gui = new Vector2(
            screenPos.x,
            Screen.height - screenPos.y
        );

        foreach (var h in all)
        {
            if (h.player != null && h.hitRect.Contains(gui))
                return true;
        }

        return false;
    }

    // POSICIÓN

    void UpdatePromptRect()
    {
        var cam = Camera.main;

        if (cam == null)
        {
            promptRect = hitRect = Rect.zero;
            return;
        }

        Vector3 sp = cam.WorldToScreenPoint(
            transform.position + Vector3.up * promptHeight
        );

        float size = Mathf.Max(
            16f,
            Screen.height * promptSize
        );

        promptRect = new Rect(
            sp.x - size * 0.5f,
            Screen.height - sp.y - size * 0.5f,
            size,
            size
        );

        float m = size * 0.15f;

        hitRect = new Rect(
            promptRect.x - m,
            promptRect.y - m,
            promptRect.width + m * 2f,
            promptRect.height + m * 2f
        );
    }

    // VISTA

    void UpdateView()
    {
        if (promptRect.width <= 0f)
        {
            if (viewRoot != null)
                viewRoot.SetActive(false);

            return;
        }

        if (viewRoot == null)
            BuildView();

        if (!viewRoot.activeSelf)
            viewRoot.SetActive(true);

        float size = promptRect.height;

        rootRt.anchoredPosition = new Vector2(
            promptRect.x,
            Screen.height - promptRect.yMax
        );

        rootRt.sizeDelta = new Vector2(size, size);

        float circleSize = size * 0.45f;

        circleRt.sizeDelta =
        ringRt.sizeDelta =
        fillRt.sizeDelta =
        letterRt.sizeDelta =
            new Vector2(circleSize, circleSize);

        letterText.fontSize = Mathf.RoundToInt(size * 0.15f);

        fillImage.color = fillColor;
        fillImage.fillAmount = progress;
        fillImage.enabled = progress > 0.001f;
    }

    void BuildView()
    {
        EnsureResources();

        viewRoot = new GameObject(
            "HoldPrompt_" + name,
            typeof(RectTransform)
        );

        viewRoot.transform.SetParent(
            GetCanvas().transform,
            false
        );

        rootRt = viewRoot.GetComponent<RectTransform>();
        SetAnchors(rootRt);

        circleRt = NewImage(
            "Fondo",
            discSprite,
            new Color(0f, 0f, 0f, 0.7f),
            out _
        );

        ringRt = NewImage(
            "Anillo",
            ringSprite,
            new Color(1f, 1f, 1f, 0.3f),
            out _
        );

        fillRt = NewImage(
            "Progreso",
            ringSprite,
            fillColor,
            out fillImage
        );

        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Radial360;
        fillImage.fillOrigin = (int)Image.Origin360.Top;
        fillImage.fillClockwise = true;
        fillImage.fillAmount = 0f;

        letterRt = NewText(
            "Tecla",
            "E",
            TextAnchor.MiddleCenter,
            out letterText
        );
    }

    RectTransform NewImage(
        string childName,
        Sprite sprite,
        Color color,
        out Image image)
    {
        var go = new GameObject(
            childName,
            typeof(RectTransform),
            typeof(Image)
        );

        go.transform.SetParent(rootRt, false);

        var rt = go.GetComponent<RectTransform>();
        SetAnchors(rt);

        image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;

        return rt;
    }

    RectTransform NewText(
        string childName,
        string content,
        TextAnchor anchor,
        out Text text)
    {
        var go = new GameObject(
            childName,
            typeof(RectTransform),
            typeof(Text),
            typeof(Outline)
        );

        go.transform.SetParent(rootRt, false);

        var rt = go.GetComponent<RectTransform>();
        SetAnchors(rt);

        text = go.GetComponent<Text>();
        text.font = uiFont;
        text.text = content;
        text.fontStyle = FontStyle.Bold;
        text.alignment = anchor;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        var outline = go.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        outline.effectDistance = new Vector2(2f, -2f);

        return rt;
    }

    static void SetAnchors(RectTransform rt)
    {
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        rt.pivot = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    static Canvas GetCanvas()
    {
        if (canvas != null)
            return canvas;

        var go = new GameObject("HoldInteractCanvas");

        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        return canvas;
    }

    static void EnsureResources()
    {
        if (uiFont == null)
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (uiFont == null)
                uiFont = Font.CreateDynamicFontFromOSFont("Arial", 32);
        }

        if (discSprite == null)
            discSprite = MakeSprite(0f, 0.5f);

        if (ringSprite == null)
            ringSprite = MakeSprite(0.39f, 0.49f);
    }

    static Sprite MakeSprite(float inner, float outer)
    {
        const int s = 128;

        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };

        var px = new Color32[s * s];

        float c = s * 0.5f;
        float rIn = inner * s;
        float rOut = outer * s;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(
                    new Vector2(x + 0.5f, y + 0.5f),
                    new Vector2(c, c)
                );

                float a = Mathf.Clamp01((rOut - d) / 1.5f);

                if (inner > 0f)
                {
                    a = Mathf.Min(
                        a,
                        Mathf.Clamp01((d - rIn) / 1.5f)
                    );
                }

                px[y * s + x] = new Color32(
                    255,
                    255,
                    255,
                    (byte)(a * 255f)
                );
            }
        }

        tex.SetPixels32(px);
        tex.Apply();

        var sprite = Sprite.Create(
            tex,
            new Rect(0, 0, s, s),
            new Vector2(0.5f, 0.5f),
            100f
        );

        sprite.hideFlags = HideFlags.HideAndDontSave;

        return sprite;
    }
}
