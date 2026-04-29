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
   public enum NPCState
    {
        Arrive,
        SearchingForTable,
        WaitingForFood,
        Eating,
        LeavingHappy,
        LeavingAngry,
        TalkingToPlayer
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
        //walk through door
        SearchForTable();
    }

    private void SearchForTable()
    {
        currentState = NPCState.SearchingForTable;
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
        // walk outside (hem de fer navmesh albert epstein)
        Destroy(gameObject, 3f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
