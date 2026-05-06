using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class NPCController : MonoBehaviour
{
   [Header ("NPC Info")]
   [Tooltip ("Drag custom NPC Data ScriptableObject here.")]
   public NPCData data;
   [Header ("Current State (read only)")]
   public float currentPatience;
   public NPCState currentState;
   private Chair assignedChair;
   [SerializeField] private NavMeshAgent agent;
   [SerializeField] private Transform tavernEntry;
   [SerializeField] private Transform spawnPoint;
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
          if(agent == null)
        {
            GetComponent<NavMeshAgent>();
        }
    }
    private void Start()
    {
        if(data != null)
        {
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

        if(data.faction == Faction.North) 
        {
            //aqui fem algo per diferenciarlos tipo canviar material o algo
        }
        else if(data.faction == Faction.South) 
        {
            //aqui fem algo per diferenciarlos tipo canviar material o algo
        }

        Arrive();
    }

    private void Arrive()
    {
        agent.SetDestination(tavernEntry.position);
        SearchForChair();
    }

    private void SearchForChair()
    {
        currentState = NPCState.SearchingForChair;
        ChairType preferredType = (data.faction == Faction.North) ? ChairType.Cold : ChairType.Warm;
        
        bool isInEnemyTerritory;
        Chair assignedChair = ChairManager.Instance.FindBestAvailableChair(preferredType, out isInEnemyTerritory);

        if(assignedChair != null)
        {
            if (isInEnemyTerritory)
            {
                Debug.Log($"{data.characterName} + hates this zone. Patience decreases.");
                ModifyPatience(-20f);
            }
            else
            {
                Debug.Log($"{data.characterName} + likes this zone. Patience increases.");
                ModifyPatience(20f);
            }
            
            agent.SetDestination(assignedChair.transform.position);
            currentState = NPCState.WalkingToChair;
        }
        else
        {
            Debug.Log($"Local lleno! {data.characterName} se va enfadado.");
            currentState = NPCState.LeavingAngry;
            GetAngryAndLeave();
        }
    }

    //GAMEPLAY

    public void ModifyPatience(float amount)
    {
        currentPatience += amount;
        currentPatience = Mathf.Clamp(currentPatience, 0, data.maxPatience);

        CheckPatienceLevel();
    }

    private void CheckPatienceLevel()
    {
        if(currentPatience <= 0 && currentState != NPCState.LeavingAngry)
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

        if(foodServed == data.foodPreference)
        {
            Debug.Log("Correct food!\n" + data.characterName + " is enjoying their food");
            ModifyPatience(20f);
            currentState = NPCState.Eating;
        }
        else
        {
            Debug.Log("Wrong food!\n" + data.characterName + " is NOT enjoying their food");
            ModifyPatience(-40f);
            currentState = NPCState.Eating;
        }
    }


    private void GetAngryAndLeave()
    {
        currentState = NPCState.LeavingAngry;

        Debug.Log(data.characterName + " from the " + data.faction + "is leaving ANGRY");

        // decrease player rep with that faction
        agent.SetDestination(spawnPoint.position);
        if(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
             Destroy(gameObject, 3f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(currentState == NPCState.WalkingToChair)
        {
            if(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                currentState = NPCState.WaitingForFood;
                WaitForFood();
            }
        }
    }

    private void WaitForFood()
    {
        throw new NotImplementedException();
    }
}
