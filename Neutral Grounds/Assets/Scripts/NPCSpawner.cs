using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("Spawning Setup")]
    [Tooltip("Drag your NPC Prefab from the Project folder here")]
    public GameObject npcPrefab;
    
    [Tooltip("Drag the Empty GameObject where they should appear")]
    public Transform spawnPoint;
    
    [Tooltip("Drag the Exit Door here so the spawner can give it to the NPC")]
    public Transform exitDoor;

    private void Start()
    {
        // For testing, let's spawn one NPC immediately when the game starts.
        // Later, you can call this method from a timer or button!
        SpawnNPC();
    }

    public void SpawnNPC()
    {
        if (npcPrefab == null || spawnPoint == null)
        {
            Debug.LogError("Spawner is missing the prefab or spawn point!");
            return;
        }

        // 1. Create the NPC at the spawn point's position and rotation
        GameObject newNPC = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);

        // 2. Grab the NPCController from the clone we just created
        NPCController controller = newNPC.GetComponent<NPCController>();

        // 3. Hand the clone the exit door reference!
        if (controller != null)
        {
            controller.spawnPoint = exitDoor;
            Debug.Log($"Spawned a new NPC and gave them the exit door location.");
        }
    }
}