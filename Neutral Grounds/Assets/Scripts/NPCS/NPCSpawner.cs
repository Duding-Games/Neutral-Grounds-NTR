using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("Spawning Setup")]
    [Tooltip("Pon aquí todos los diferentes tipos de NPCs que tengas")]
    public GameObject[] npcPrefabs; 
    public Transform spawnPoint;
    public Transform exitDoor;

    [Header("Timer Setup")]
    public float timeBetweenSpawns = 1f;
    private float spawnTimer;

    private void Update()
    {
        if (GameManager.Instance.currentState == GameManager.GameState.TavernOpen)
        {
            spawnTimer -= Time.deltaTime;
            
            if (spawnTimer <= 0f)
            {
                SpawnNPC();
                spawnTimer = timeBetweenSpawns; 
            }
        }
    }

    public void SpawnNPC()
    {
        if (npcPrefabs == null || npcPrefabs.Length == 0 || spawnPoint == null) return;

        // Elige un NPC aleatorio de la lista
        int randomIndex = Random.Range(0, npcPrefabs.Length);
        GameObject prefabToSpawn = npcPrefabs[randomIndex];

        GameObject newNPC = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        NPCController controller = newNPC.GetComponent<NPCController>();

        if (controller != null)
        {
            controller.spawnPoint = exitDoor;
        }
    }
}