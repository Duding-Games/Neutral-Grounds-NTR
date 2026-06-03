using System;
using System.Collections.Generic; 
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
    public float patienceLossPerSecond = 1f;
    public float timeToEat = 60f;
    private float currentEatingTimer;
    
    private float interactionTimer = 3f; 

    [Header("Visual Feedback")]
    public Slider patienceBar; 
    public GameObject happyParticlePrefab;
    public GameObject angryParticlePrefab;

    private Chair assignedChair;
    private GameObject currentFoodOnTable; 

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform tavernEntry;
    [SerializeField] public Transform spawnPoint;

    private bool isHovering = false; 
    private List<RumorData> rumorsHeard = new List<RumorData>();

    public enum NPCState
    {
        Arrive, SearchingForChair, WalkingToChair, WaitingForFood, Eating, LeavingHappy, LeavingAngry, TalkingToPlayer
    }

    private void Awake() { if (agent == null) agent = GetComponent<NavMeshAgent>(); }

    private void Start()
    {
        if (data != null)
        {
            if (GameManager.Instance != null) GameManager.Instance.RegisterCustomer();
            InitializeNPC();
        }
        else Debug.LogError("Missing NPC Data for " + gameObject.name);
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
        if (amount > 0 && happyParticlePrefab != null) Destroy(Instantiate(happyParticlePrefab, spawnPosition, Quaternion.identity), 2f);
        if (amount < 0 && angryParticlePrefab != null) Destroy(Instantiate(angryParticlePrefab, spawnPosition, Quaternion.identity), 2f);

        CheckPatienceLevel();
    }

    private void UpdatePatienceUI() { if (patienceBar != null) patienceBar.value = currentPatience / data.maxPatience; }

    private void CheckPatienceLevel() { if (currentPatience <= 0 && currentState != NPCState.LeavingAngry) GetAngryAndLeave(); }

    public void ReceiveOrder(FoodPreference foodServed, GameObject foodItem)
    {
        if (currentState != NPCState.WaitingForFood) return;

        currentFoodOnTable = foodItem;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        
        if (foodServed == data.foodPreference)
        {
            ModifyPatience(20f);
            currentState = NPCState.Eating;
            currentEatingTimer = timeToEat; 
        }
        else
        {
            ModifyPatience(-40f); 
            GetAngryAndLeave(); 
        }
    }

    private void GetAngryAndLeave()
    {
        currentState = NPCState.LeavingAngry;
        if (assignedChair != null) assignedChair.isOccupied = false;
        if (currentFoodOnTable != null) Destroy(currentFoodOnTable);
        
        if (spawnPoint != null) agent.SetDestination(spawnPoint.position);
        else Debug.LogWarning("NPC no tiene spawnPoint para salir.");
    }

    private void LeaveHappy()
    {
        currentState = NPCState.LeavingHappy;
        if (assignedChair != null) assignedChair.isOccupied = false;
        if (currentFoodOnTable != null) Destroy(currentFoodOnTable);
        
        if (spawnPoint != null) agent.SetDestination(spawnPoint.position);
        else Debug.LogWarning("NPC no tiene spawnPoint para salir.");
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

        // --- EL CONGELADOR DE TIEMPO ---
        // Si hay una conversación en curso, no restamos paciencia ni avanzamos sus relojes.
        if (ConversationManager.Instance != null && ConversationManager.Instance.IsConversationActive)
            return; 

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
            // Usamos 0.5f de margen para asegurarnos de que caminan hasta la puerta de verdad
            if (!agent.pathPending && agent.remainingDistance <= 0.5f) 
            {
                Destroy(gameObject);
            }
        }
    }

    private void CheckPlayerInputs()
    {
        if (!isHovering) return;

        if (KnowledgeManager.Instance != null && KnowledgeManager.Instance.isMenuOpen) return;

        if (currentState == NPCState.WaitingForFood)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ReceiveOrder(FoodPreference.Synthetic, null);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) ReceiveOrder(FoodPreference.Organic, null);
        }

        if (ConversationManager.Instance != null && ConversationManager.Instance.IsConversationActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            NPCConversation[] convoList = (currentPatience > 50f) ? data.happyConversations : data.angryConversations;
            if (convoList != null && convoList.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, convoList.Length);
                ConversationManager.Instance.StartConversation(convoList[randomIndex]);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (KnowledgeManager.Instance != null) KnowledgeManager.Instance.OpenRumorMenu(this);
        }
    }

    public void ReceiveRumor(RumorData rumor)
    {
        if (rumorsHeard.Contains(rumor))
        {
            if (data.alreadyKnownConversation != null && ConversationManager.Instance != null)
            {
                ConversationManager.Instance.StartConversation(data.alreadyKnownConversation);
            }
            return; 
        }

        rumorsHeard.Add(rumor);

        if (data.isVIP)
        {
            float vipPatienceChange = (data.faction == Faction.North) ? rumor.northPatienceChange : rumor.southPatienceChange;
            ModifyPatience(vipPatienceChange * 1.5f);

            NPCConversation vipConvo = null;

            switch (data.vipIdentity)
            {
                case VIPCharacter.Viktor: vipConvo = rumor.viktorReactionChat; break;
                case VIPCharacter.Valeria: vipConvo = rumor.valeriaReactionChat; break;
                case VIPCharacter.Irina: vipConvo = rumor.irinaReactionChat; break;
                case VIPCharacter.Lorenzo: vipConvo = rumor.lorenzoReactionChat; break;
            }

            if (vipConvo != null && ConversationManager.Instance != null)
            {
                ConversationManager.Instance.StartConversation(vipConvo);
                return; 
            }
        }

        if (data.faction == Faction.North)
        {
            if (!data.isVIP) ModifyPatience(rumor.northPatienceChange); 
            if (rumor.northReactionChat != null && ConversationManager.Instance != null)
                ConversationManager.Instance.StartConversation(rumor.northReactionChat);
        }
        else if (data.faction == Faction.South)
        {
            if (!data.isVIP) ModifyPatience(rumor.southPatienceChange); 
            if (rumor.southReactionChat != null && ConversationManager.Instance != null)
                ConversationManager.Instance.StartConversation(rumor.southReactionChat);
        }
    }

    private void OnDestroy() { if (GameManager.Instance != null) GameManager.Instance.UnregisterCustomer(); }
}