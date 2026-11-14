using System.Collections;
using UnityEngine;

// Trigger de la sala del jefe (nivel 3): cuando el jugador entra,
// baja la puerta para encerrarlo y despierta al jefe.
[RequireComponent(typeof(BoxCollider2D))]
public class GameManager : MonoBehaviour
{
    // Referencias y ajustes para abrir la puerta y activar al jefe.
    [SerializeField] private Transform doorTilemap;
    [SerializeField] private GridLayout grid;
    [SerializeField] private BossComportamiento boss;
    [SerializeField, Min(1)] private int blocksToMove = 6;

    private GameObject finJuegoCanvas;

    private Collider2D bossCollider;
    private bool finJuegoMostrado;

    // Para que la puerta solo se mueva una vez
    private bool doorHasMoved;
    private Rigidbody2D bossRigidbody;

    // Comprueba las referencias y deja al jefe inactivo hasta abrir la puerta.
    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;

        if (doorTilemap == null || grid == null || boss == null)
        {
            Debug.LogError("GameManager necesita referencias al Tilemap Puerta, al Grid y al jefe.", this);
            enabled = false;
            return;
        }

        bossRigidbody = boss.GetComponent<Rigidbody2D>();
        bossRigidbody.simulated = false;
        boss.enabled = false;

        bossCollider = boss.GetComponent<Collider2D>();

        finJuegoCanvas = GameObject.FindGameObjectWithTag("FinJuego");

        if (finJuegoCanvas == null)
        {
            Debug.LogError("No se encontró ningún objeto con el tag FinJuego.", this);
            enabled = false;
            return;
        }

        finJuegoCanvas.SetActive(false);
    }

    // Al entrar el jugador, mueve el Tilemap y activa al jefe una sola vez.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (doorHasMoved || !other.CompareTag("Player"))
        {
            return;
        }

        // Calcula cuánto mide en el mundo bajar 'blocksToMove' casillas y mueve la puerta
        Vector3 startCell = grid.CellToWorld(Vector3Int.zero);
        Vector3 destinationCell = grid.CellToWorld(Vector3Int.down * blocksToMove);
        doorTilemap.position += destinationCell - startCell;
        // Despierta al jefe: activa sus físicas y su comportamiento
        bossRigidbody.simulated = true;
        boss.enabled = true;
        doorHasMoved = true;
    }

    private void Update()
    {
        if (!finJuegoMostrado && !bossCollider.enabled)
        {
            MostrarFinJuego();
        }
    }

    private void MostrarFinJuego()
    {
        finJuegoMostrado = true;
        finJuegoCanvas.SetActive(true);
        StartCoroutine(TerminarJuego());
    }

    private IEnumerator TerminarJuego()
    {
        yield return new WaitForSecondsRealtime(3f);

    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
        }
}
