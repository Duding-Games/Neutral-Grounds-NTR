using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using DialogueEditor;

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
    
    private float interactionTimer = 3f; 

    [Header("Visual Feedback")]
    public Slider patienceBar; 
    public GameObject happyParticlePrefab;
    public GameObject angryParticlePrefab;

    private Chair assignedChair;
    private GameObject currentFoodOnTable; // Variable para guardar el plato físico

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform tavernEntry;
    [SerializeField] public Transform spawnPoint;

    private bool isHovering = false; 

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
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (data != null)
        {
            if (GameManager.Instance != null) GameManager.Instance.RegisterCustomer();
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
        UpdatePatienceUI(); 
        currentState = NPCState.Arrive;
        Arrive();
    }

    private void Arrive()
    {
        if (tavernEntry != null) agent.SetDestination(tavernEntry.position);
        else SearchForChair();
    }

    private void SearchForChair()
    {
        currentState = NPCState.SearchingForChair;
        ChairType preferredType = (data.faction == Faction.North) ? ChairType.Cold : ChairType.Warm;
        bool isInEnemyTerritory;
        assignedChair = ChairManager.Instance.FindBestAvailableChair(preferredType, out isInEnemyTerritory);

        if (assignedChair != null)
        {
            if (isInEnemyTerritory) ModifyPatience(-20f);
            else ModifyPatience(20f);

            agent.SetDestination(assignedChair.transform.position);
            currentState = NPCState.WalkingToChair;
        }
        else GetAngryAndLeave();
    }

    public void ModifyPatience(float amount)
    {
        currentPatience += amount;
        currentPatience = Mathf.Clamp(currentPatience, 0, data.maxPatience);
        UpdatePatienceUI(); 

        Vector3 spawnPosition = transform.position + Vector3.up * 2f;
        if (amount > 0 && happyParticlePrefab != null) 
        {
            GameObject particles = Instantiate(happyParticlePrefab, spawnPosition, Quaternion.identity);
            Destroy(particles, 2f);
        }
        if (amount < 0 && angryParticlePrefab != null) 
        {
            GameObject particles = Instantiate(angryParticlePrefab, spawnPosition, Quaternion.identity);
            Destroy(particles, 2f);
        }

        CheckPatienceLevel();
    }

    private void UpdatePatienceUI()
    {
        if (patienceBar != null) patienceBar.value = currentPatience / data.maxPatience;
    }

    private void CheckPatienceLevel()
    {
        if (currentPatience <= 0 && currentState != NPCState.LeavingAngry) GetAngryAndLeave();
    }

    // Actualizado para recibir también el GameObject del plato
    public void ReceiveOrder(FoodPreference foodServed, GameObject foodItem)
    {
        if (currentState != NPCState.WaitingForFood) return;

        // Guardamos la referencia a la comida en la mesa para poder borrarla luego
        currentFoodOnTable = foodItem;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (foodServed == data.foodPreference)
        {
            ModifyPatience(20f);
            if (meshRenderer != null) meshRenderer.material.color = Color.green;
            currentState = NPCState.Eating;
            currentEatingTimer = timeToEat; 
        }
        else
        {
            ModifyPatience(-40f); 
            if (meshRenderer != null) meshRenderer.material.color = Color.red;
            GetAngryAndLeave(); 
        }
    }

    private void GetAngryAndLeave()
    {
        currentState = NPCState.LeavingAngry;
        if (assignedChair != null) assignedChair.isOccupied = false;
        
        // Destruimos el plato si lo tenía
        if (currentFoodOnTable != null) Destroy(currentFoodOnTable);
        
        if (spawnPoint != null) agent.SetDestination(spawnPoint.position);
    }

    private void LeaveHappy()
    {
        currentState = NPCState.LeavingHappy;
        if (assignedChair != null) assignedChair.isOccupied = false;
        
        // Destruimos el plato al irse contento
        if (currentFoodOnTable != null) Destroy(currentFoodOnTable);
        
        if (spawnPoint != null) agent.SetDestination(spawnPoint.position);
    }

    private void CheckSurroundings()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider col in nearbyColliders)
        {
            NPCController otherNPC = col.GetComponent<NPCController>();
            if (otherNPC != null && otherNPC != this) 
            {
                if (data.trait == NPCTrait.Loner) ModifyPatience(-2f); 
                else if (data.trait == NPCTrait.Grumpy && otherNPC.data.faction != data.faction) ModifyPatience(-5f); 
                else if (data.trait == NPCTrait.Chatty && otherNPC.data.faction == data.faction) ModifyPatience(2f); 
            }
        }
    }

    private void OnMouseEnter() { isHovering = true; }
    private void OnMouseExit() { isHovering = false; }

    void Update()
    {
        CheckPlayerInputs();

        interactionTimer -= Time.deltaTime;
        if (interactionTimer <= 0)
        {
            CheckSurroundings();
            interactionTimer = 3f; 
        }

        if (currentState == NPCState.Arrive)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) SearchForChair();
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
            if (currentEatingTimer <= 0) LeaveHappy();
        }
        else if (currentState == NPCState.LeavingAngry || currentState == NPCState.LeavingHappy)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) Destroy(gameObject);
        }
    }

    private void CheckPlayerInputs()
    {
        if (!isHovering) return;

        if (currentState == NPCState.WaitingForFood)
        {
            // Atajos de teclado para debug: les pasamos 'null' porque no hay plato físico
            if (Input.GetKeyDown(KeyCode.Alpha1)) ReceiveOrder(FoodPreference.Synthetic, null);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) ReceiveOrder(FoodPreference.Organic, null);
        }

        if (ConversationManager.Instance != null && ConversationManager.Instance.IsConversationActive) return;

        // Click Izquierdo: Charla Normal
        if (Input.GetMouseButtonDown(0))
        {
            NPCConversation[] convoList = (currentPatience > 50f) ? data.happyConversations : data.angryConversations;
            if (convoList != null && convoList.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, convoList.Length);
                ConversationManager.Instance.StartConversation(convoList[randomIndex]);
            }
        }

        // Click Derecho: Compartir Rumor automáticamente
        if (Input.GetMouseButtonDown(1))
        {
            if (KnowledgeManager.Instance != null && KnowledgeManager.Instance.knownRumors.Count > 0)
            {
                // Cogemos el último de la lista
                RumorData rumorToShare = KnowledgeManager.Instance.knownRumors[KnowledgeManager.Instance.knownRumors.Count - 1];
                ShareRumor(rumorToShare);
            }
            else
            {
                Debug.Log("Todavía no sabes ningún rumor.");
            }
        }
    }

    private void ShareRumor(RumorData rumor)
    {
        Debug.Log($"Contando el rumor '{rumor.rumorName}' a {data.characterName}");

        if (data.faction == Faction.North)
        {
            ModifyPatience(rumor.northPatienceChange); 
            if (rumor.northReactionChat != null && ConversationManager.Instance != null)
            {
                ConversationManager.Instance.StartConversation(rumor.northReactionChat);
            }
        }
        else if (data.faction == Faction.South)
        {
            ModifyPatience(rumor.southPatienceChange); 
            if (rumor.southReactionChat != null && ConversationManager.Instance != null)
            {
                ConversationManager.Instance.StartConversation(rumor.southReactionChat);
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.UnregisterCustomer();
    }
}