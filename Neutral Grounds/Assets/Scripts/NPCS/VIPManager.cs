using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VIPSchedule
{
    public string characterName;      
    public GameObject vipPrefab;      
    [Range(0f, 1f)]
    public float timeToSpawn;         
    
    [HideInInspector]
    public bool hasAppearedToday = false; 
}

public class VIPManager : MonoBehaviour
{
    public static VIPManager Instance { get; private set; }

    [Header("Referencias")]
    public DayNightCycle dayNightCycle; 
    public Transform tavernSpawnPoint;  

    [Header("Horarios de los VIPs")]
    public List<VIPSchedule> vipSchedules = new List<VIPSchedule>();

    private float previousTimeOfDay = 0f;
    private int currentDay = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (dayNightCycle == null) return;

        float currentTime = dayNightCycle.progresoDia;

        if (currentTime < previousTimeOfDay) StartNewDay();

        foreach (VIPSchedule vip in vipSchedules)
        {
            if (!vip.hasAppearedToday && currentTime >= vip.timeToSpawn)
            {
                SpawnVIP(vip);
            }
        }

        previousTimeOfDay = currentTime;
    }

    private void SpawnVIP(VIPSchedule vip)
    {
        vip.hasAppearedToday = true;

        if (vip.vipPrefab != null && tavernSpawnPoint != null)
        {
            GameObject newVip = Instantiate(vip.vipPrefab, tavernSpawnPoint.position, Quaternion.identity);
            
            // EL ARREGLO: Le decimos al VIP dónde está la puerta para que sepa por dónde irse
            NPCController controller = newVip.GetComponent<NPCController>();
            if (controller != null)
            {
                controller.spawnPoint = tavernSpawnPoint;
            }

            Debug.Log($"[VIP Manager] ¡{vip.characterName} acaba de llegar a la taberna!");
        }
        else
        {
            Debug.LogWarning($"[VIP Manager] Faltan referencias para invocar a {vip.characterName}.");
        }
    }

    private void StartNewDay()
    {
        currentDay++;
        Debug.Log($"[VIP Manager] --- COMIENZA EL DÍA {currentDay} ---");

        foreach (VIPSchedule vip in vipSchedules)
        {
            vip.hasAppearedToday = false;
        }
    }
}