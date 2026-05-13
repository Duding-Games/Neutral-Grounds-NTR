using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ChairManager : MonoBehaviour
{
    public static ChairManager Instance {get; private set;}

    private Chair[] allChairs;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);

        allChairs = FindObjectsByType<Chair>(FindObjectsSortMode.None);
    }
   
   public Chair FindBestAvailableChair(ChairType preferredType, out bool gotWrongType)
    {
        gotWrongType = false;
        Chair fallbackChair = null;

        foreach (Chair chair in allChairs)
        {
            if (!chair.isOccupied)
            {
                if(chair.type == preferredType)
                {
                    chair.isOccupied = true;
                    return chair;
                }
                else if(fallbackChair == null)
                {
                    fallbackChair = chair;
                }
            }
        }

        if(fallbackChair != null)
        {
            fallbackChair.isOccupied = true;
            gotWrongType = true;
            return fallbackChair;
        }
        
        return null;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
