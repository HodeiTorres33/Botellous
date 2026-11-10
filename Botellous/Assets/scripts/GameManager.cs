using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GameManager : MonoBehaviour
{
    // Referencias y ajustes para abrir la puerta y activar al jefe.
    [SerializeField] private Transform doorTilemap;
    [SerializeField] private GridLayout grid;
    [SerializeField] private BossComportamiento boss;
    [SerializeField, Min(1)] private int blocksToMove = 6;

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
    }

    // Al entrar el jugador, mueve el Tilemap y activa al jefe una sola vez.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (doorHasMoved || !other.CompareTag("Player"))
        {
            return;
        }

        Vector3 startCell = grid.CellToWorld(Vector3Int.zero);
        Vector3 destinationCell = grid.CellToWorld(Vector3Int.down * blocksToMove);
        doorTilemap.position += destinationCell - startCell;
        bossRigidbody.simulated = true;
        boss.enabled = true;
        doorHasMoved = true;
    }
}
