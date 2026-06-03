using UnityEngine;
using DialogueEditor; 

public enum Faction { North, South }
public enum FoodPreference { Synthetic, Organic }
public enum NPCTrait { Grumpy, Cheerful, Chatty, Loner } 
public enum VIPCharacter { None, Viktor, Valeria, Irina, Lorenzo }

[CreateAssetMenu(fileName = "NewNPC", menuName = "NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("General Info")]
    public string characterName;
    public Faction faction;
    public FoodPreference foodPreference;
    public NPCTrait trait; 

    [Header("VIP Settings")]
    public bool isVIP = false;
    [Tooltip("Solo importa si la casilla de arriba está marcada")]
    public VIPCharacter vipIdentity = VIPCharacter.None;

    [Header("Base Stats")]
    public float maxPatience = 100f;
    [Tooltip("If this value is high the NPC gets mad at noises")]
    public float noiseSensitivity; 

    [Header("Emergent Dialogues")]
    public NPCConversation[] happyConversations;
    public NPCConversation[] angryConversations;

    [Header("Rumor Reactions")]
    public NPCConversation alreadyKnownConversation;
}