using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("Spawning Setup")]
    public GameObject npcPrefab;
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
        if (npcPrefab == null || spawnPoint == null) return;

        GameObject newNPC = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);
        NPCController controller = newNPC.GetComponent<NPCController>();

        if (controller != null)
        {
            controller.spawnPoint = exitDoor;
        }
    }
}