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

    [Header("Diálogos VIP Específicos")]
    public NPCConversation viktorReactionChat;
    public NPCConversation valeriaReactionChat;
    public NPCConversation irinaReactionChat;
    public NPCConversation lorenzoReactionChat;

    // --- LA FUNCIÓN RECUPERADA ---
    // Esta es la función que se había borrado y que llama el Dialogue Editor
    public void TriggerRumor()
    {
        if (KnowledgeManager.Instance != null)
        {
            KnowledgeManager.Instance.LearnRumor(this);
        }
        else
        {
            Debug.LogWarning("Intentando aprender un rumor, pero el KnowledgeManager no está en la escena.");
        }
    }
}