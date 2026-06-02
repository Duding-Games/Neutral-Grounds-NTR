using UnityEngine;
using DialogueEditor; // Necesario para guardar las conversaciones aquí

public enum Faction { North, South }
public enum FoodPreference { Synthetic, Organic }
public enum NPCTrait { Grumpy, Cheerful, Chatty, Loner } 

[CreateAssetMenu(fileName = "NewNPC", menuName = "NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("General Info")]
    public string characterName;
    public Faction faction;
    public FoodPreference foodPreference;
    public NPCTrait trait; // El rasgo define cómo interactúa con otros

    [Header("Base Stats")]
    public float maxPatience = 100f;
    [Tooltip("If this value is high the NPC gets mad at noises")]
    public float noiseSensitivity; // Cambiado a float, los Sliders son solo para UI

    [Header("Emergent Dialogues")]
    [Tooltip("Diálogos cuando la paciencia está por encima de 50")]
    public NPCConversation[] happyConversations;
    [Tooltip("Diálogos cuando la paciencia está por debajo de 50")]
    public NPCConversation[] angryConversations;
}