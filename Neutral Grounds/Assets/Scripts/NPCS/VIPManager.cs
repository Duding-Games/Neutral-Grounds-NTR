using System.Collections.Generic;
using UnityEngine;

// Esta clase nos permite configurar a cada VIP en el Inspector
[System.Serializable]
public class VIPSchedule
{
    public string characterName;      // Ej: "Viktor"
    public GameObject vipPrefab;      // El Prefab del NPC de Viktor
    [Range(0f, 1f)]
    [Tooltip("Momento del día en el que aparece (0.2 es mañana, 0.8 es tarde)")]
    public float timeToSpawn;         
    
    [HideInInspector]
    public bool hasAppearedToday = false; // Control interno para no invocarlo dos veces
}

public class VIPManager : MonoBehaviour
{
    public static VIPManager Instance { get; private set; }

    [Header("Referencias")]
    public DayNightCycle dayNightCycle; // Arrastra tu script de día y noche aquí
    public Transform tavernSpawnPoint;  // La puerta de entrada donde aparecen

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

        // 1. Detectar un Nuevo Día
        // Si el tiempo actual es menor que el del frame anterior, significa que 
        // el progresoDia ha llegado a 1 y ha vuelto a 0 (reinicio de ciclo).
        if (currentTime < previousTimeOfDay)
        {
            StartNewDay();
        }

        // 2. Comprobar si es la hora de algún VIP
        foreach (VIPSchedule vip in vipSchedules)
        {
            // Si no ha aparecido hoy y el reloj ha superado su hora de llegada...
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
            Instantiate(vip.vipPrefab, tavernSpawnPoint.position, Quaternion.identity);
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

        // Reseteamos el estado de todos los VIPs para que puedan volver a venir hoy
        foreach (VIPSchedule vip in vipSchedules)
        {
            vip.hasAppearedToday = false;
        }
    }
}