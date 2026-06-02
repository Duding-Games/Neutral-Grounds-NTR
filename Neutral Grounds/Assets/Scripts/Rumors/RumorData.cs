using UnityEngine;
using DialogueEditor;

[CreateAssetMenu(fileName = "NewRumor", menuName = "Tavern/Rumor Data")]
public class RumorData : ScriptableObject
{
    [Header("Información del Rumor")]
    public string rumorName; 
    [TextArea]
    public string description; 

    [Header("Reacciones por Facción")]
    public int northPatienceChange; 
    public int southPatienceChange;

    [Header("Diálogos de Respuesta (Opcional)")]
    public NPCConversation northReactionChat;
    public NPCConversation southReactionChat;

    // --- EL PUENTE PARA EL DIALOGUE EDITOR ---
    public void TriggerLearnRumor()
    {
        if (KnowledgeManager.Instance != null)
        {
            KnowledgeManager.Instance.LearnRumor(this);
        }
        else
        {
            Debug.LogError("No se ha encontrado el KnowledgeManager en la escena.");
        }
    }
}