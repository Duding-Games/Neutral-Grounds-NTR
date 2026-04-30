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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
