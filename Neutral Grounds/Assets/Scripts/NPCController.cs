using System;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    [Header("NPC Info")]
    public NPCData data;

    [Header("Current State (read only)")]
    public float currentPatience;
    public NPCState currentState;

    [Header("Timers")]
    [Tooltip("How much patience they lose per second while waiting")]
    public float patienceLossPerSecond = 5f;
    [Tooltip("How long they stay at the table to eat before leaving")]
    public float timeToEat = 3f;
    private float currentEatingTimer;

    private Chair assignedChair;

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform tavernEntry;
    [SerializeField] public Transform spawnPoint;

    public enum NPCState
    {
        Arrive,
        SearchingForChair,
        WalkingToChair,
        WaitingForFood,
        Eating,
        LeavingHappy,
        LeavingAngry,
        TalkingToPlayer
    }

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }

    private void Start()
    {
        if (data != null)
        {
            GameManager.Instance.RegisterCustomer();
            InitializeNPC();
        }
        else
        {
            Debug.LogError("Missing NPC Data for " + gameObject.name);
        }
    }

    private void InitializeNPC()
    {
        currentPatience = data.maxPatience;
        currentState = NPCState.Arrive;

        if (data.faction == Faction.North)
        {
        }
        else if (data.faction == Faction.South)
        {
        }

        Arrive();
    }

    private void Arrive()
    {
        if (tavernEntry != null)
        {
            agent.SetDestination(tavernEntry.position);
        }
        else
        {
            SearchForChair();
        }
    }

    private void SearchForChair()
    {
        currentState = NPCState.SearchingForChair;
        ChairType preferredType = (data.faction == Faction.North) ? ChairType.Cold : ChairType.Warm;

        bool isInEnemyTerritory;
        assignedChair = ChairManager.Instance.FindBestAvailableChair(preferredType, out isInEnemyTerritory);

        if (assignedChair != null)
        {
            if (isInEnemyTerritory)
            {
                Debug.Log($"{data.characterName} hates this zone. Patience decreases.");
                ModifyPatience(-20f);
            }
            else
            {
                Debug.Log($"{data.characterName} likes this zone. Patience increases.");
                ModifyPatience(20f);
            }

            agent.SetDestination(assignedChair.transform.position);
            currentState = NPCState.WalkingToChair;
        }
        else
        {
            Debug.Log($"The tavern is full! {data.characterName} leaves ANGRY.");
            GetAngryAndLeave();
        }
    }

    public void ModifyPatience(float amount)
    {
        currentPatience += amount;
        currentPatience = Mathf.Clamp(currentPatience, 0, data.maxPatience);

        CheckPatienceLevel();
    }

    private void CheckPatienceLevel()
    {
        if (currentPatience <= 0 && currentState != NPCState.LeavingAngry)
        {
            GetAngryAndLeave();
        }
    }

   public void ReceiveOrder(FoodPreference foodServed)
    {
        if (currentState != NPCState.WaitingForFood)
        {
            return;
        }

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (foodServed == data.foodPreference)
        {
            Debug.Log("Correct food! " + data.characterName + " is enjoying their food");
            ModifyPatience(20f);
            
            if (meshRenderer != null) meshRenderer.material.color = Color.green;
            
            currentState = NPCState.Eating;
            currentEatingTimer = timeToEat; 
        }
        else
        {
            Debug.Log("Wrong food! " + data.characterName + " refuses to eat this!");
            ModifyPatience(-40f); 
            
            if (meshRenderer != null) meshRenderer.material.color = Color.red;
            
            GetAngryAndLeave(); 
        }
    }

    private void GetAngryAndLeave()
    {
        currentState = NPCState.LeavingAngry;

        Debug.Log(data.characterName + " from the " + data.faction + " is leaving ANGRY");

        if (assignedChair != null)
        {
            assignedChair.isOccupied = false;
        }

        if (spawnPoint != null)
        {
            agent.SetDestination(spawnPoint.position);
        }
    }

    private void LeaveHappy()
    {
        currentState = NPCState.LeavingHappy;

        Debug.Log(data.characterName + " finished eating and is leaving HAPPY!");

        if (assignedChair != null)
        {
            assignedChair.isOccupied = false;
        }

        if (spawnPoint != null)
        {
            agent.SetDestination(spawnPoint.position);
        }
    }

    void Update()
    {
        if (currentState == NPCState.Arrive)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                SearchForChair();
            }
        }
        else if (currentState == NPCState.WalkingToChair)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                currentState = NPCState.WaitingForFood;
                transform.position = assignedChair.transform.position;
            }
        }
        else if (currentState == NPCState.WaitingForFood)
        {
            ModifyPatience(-patienceLossPerSecond * Time.deltaTime);
        }
        else if (currentState == NPCState.Eating)
        {
            currentEatingTimer -= Time.deltaTime;
            if (currentEatingTimer <= 0)
            {
                LeaveHappy();
            }
        }
        else if (currentState == NPCState.LeavingAngry || currentState == NPCState.LeavingHappy)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnMouseOver()
    {
        if (currentState == NPCState.WaitingForFood)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ReceiveOrder(FoodPreference.Synthetic);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ReceiveOrder(FoodPreference.Organic);
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterCustomer();
        }
    }
}