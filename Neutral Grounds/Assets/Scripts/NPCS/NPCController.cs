using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; // Necesario para la Slider de paciencia
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
    
    // Timer para que no estén calculando vecinos cada milisegundo
    private float interactionTimer = 3f; 

    [Header("Visual Feedback")]
    public Slider patienceBar; // Arrastra aquí la barra de la UI
    public ParticleSystem happyParticles;
    public ParticleSystem angryParticles;

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
        UpdatePatienceUI(); // Actualizar la barra visual al inicio
        currentState = NPCState.Arrive;

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
        UpdatePatienceUI(); // Feedback visual instantáneo

        // Feedback de partículas
        if (amount > 0 && happyParticles != null) happyParticles.Play();
        if (amount < 0 && angryParticles != null) angryParticles.Play();

        CheckPatienceLevel();
    }

    private void UpdatePatienceUI()
    {
        if (patienceBar != null)
        {
            // El slider va de 0 a 1, así que dividimos la actual entre la máxima
            patienceBar.value = currentPatience / data.maxPatience;
        }
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

    // --- NARRATIVA EMERGENTE: INTERACCIÓN ENTRE NPCS ---
    private void CheckSurroundings()
    {
        // Creamos una esfera invisible de 5 metros para detectar otros NPCs
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 5f);
        int nearbyPeople = 0;

        foreach (Collider col in nearbyColliders)
        {
            NPCController otherNPC = col.GetComponent<NPCController>();
            if (otherNPC != null && otherNPC != this) // Si hay otro NPC y no soy yo mismo
            {
                nearbyPeople++;

                // Lógica de rasgos
                if (data.trait == NPCTrait.Loner)
                {
                    ModifyPatience(-2f); // Odia estar cerca de cualquiera
                }
                else if (data.trait == NPCTrait.Grumpy && otherNPC.data.faction != data.faction)
                {
                    ModifyPatience(-5f); // Odia estar cerca de la facción rival
                }
                else if (data.trait == NPCTrait.Chatty && otherNPC.data.faction == data.faction)
                {
                    ModifyPatience(2f); // Le encanta estar con los suyos
                }
            }
        }
    }

    void Update()
    {
        // Control del timer de interacción
        interactionTimer -= Time.deltaTime;
        if (interactionTimer <= 0)
        {
            CheckSurroundings();
            interactionTimer = 3f; // Vuelve a comprobar cada 3 segundos
        }

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
        if (Input.GetMouseButtonDown(0))
        {
            // Elige qué decir basado en su estado de ánimo (paciencia)
            NPCConversation[] convoList = (currentPatience > 50f) ? data.happyConversations : data.angryConversations;

            if (convoList != null && convoList.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, convoList.Length);
                ConversationManager.Instance.StartConversation(convoList[randomIndex]);
            }
            else
            {
                Debug.LogWarning(data.characterName + " no tiene conversaciones asignadas en su Data.");
            }
        }

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