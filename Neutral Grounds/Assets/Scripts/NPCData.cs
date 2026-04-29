using UnityEngine;
using UnityEngine.UI;

public enum Faction { North, South}
public enum FoodPreference { Synthetic, Organic}

[CreateAssetMenu(fileName = "NewNPC", menuName = "NPC Data")]

public class NPCData : ScriptableObject
{
    [Header ("General Info")]
    public string characterName;
    public Faction faction;
    public FoodPreference foodPreference;

    [Header ("Base Stats")]
    public float maxPatience = 100f;
    [Tooltip ("If this value is high the NPC gets mad at noises")]
    public Slider noiseSensitivity;

}
