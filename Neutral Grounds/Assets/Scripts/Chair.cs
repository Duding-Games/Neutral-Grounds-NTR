using UnityEngine;

public enum ChairType {Cold, Warm}
public class Chair : MonoBehaviour
{
    public ChairType type;
    public bool isOccupied = false;
}
