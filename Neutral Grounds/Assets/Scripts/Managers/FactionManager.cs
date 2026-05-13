using UnityEngine;

public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance { get; private set; }

    [Header("Reputation Scores (0 to 100)")]
    [Range(0, 100)] public int northReputation = 50; 
    [Range(0, 100)] public int southReputation = 50;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    public void AddNorthRep(int amount)
    {
        northReputation = Mathf.Clamp(northReputation + amount, 0, 100);
        Debug.Log($"North Rep increased by {amount}. Current: {northReputation}");
    }

    public void SubtractNorthRep(int amount)
    {
        northReputation = Mathf.Clamp(northReputation - amount, 0, 100);
        Debug.Log($"North Rep decreased by {amount}. Current: {northReputation}");
    }

    public void AddSouthRep(int amount)
    {
        southReputation = Mathf.Clamp(southReputation + amount, 0, 100);
        Debug.Log($"South Rep increased by {amount}. Current: {southReputation}");
    }

    public void SubtractSouthRep(int amount)
    {
        southReputation = Mathf.Clamp(southReputation - amount, 0, 100);
        Debug.Log($"South Rep decreased by {amount}. Current: {southReputation}");
    }
}