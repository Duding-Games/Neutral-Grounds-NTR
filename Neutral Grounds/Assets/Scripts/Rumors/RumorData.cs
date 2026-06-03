using UnityEngine;
using DialogueEditor;

[CreateAssetMenu(fileName = "NewRumor", menuName = "Tavern/Rumor Data")]
public class RumorData : ScriptableObject
{
    [Header("Información del Rumor")]
    public string rumorName; 
    [TextArea]
    public string description; 

    [Header("Reacciones Genéricas (Puntos)")]
    public int northPatienceChange; 
    public int southPatienceChange;

    [Header("Diálogos Genéricos por Facción")]
    public NPCConversation northReactionChat;
    public NPCConversation southReactionChat;

    // --- NUEVO: Diálogos VIP Específicos ---
    [Header("Diálogos VIP Específicos")]
    public NPCConversation viktorReactionChat;
    public NPCConversation valeriaReactionChat;
    public NPCConversation irinaReactionChat;
    public NPCConversation lorenzoReactionChat;
}