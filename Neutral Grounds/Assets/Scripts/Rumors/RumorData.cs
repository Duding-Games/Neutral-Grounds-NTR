using UnityEngine;
using DialogueEditor;

[CreateAssetMenu(fileName = "NewRumor", menuName = "Tavern/Rumor Data")]
public class RumorData : ScriptableObject
{
    [Header("Información del Rumor")]
    public string rumorName; 
    [TextArea]
    public string description; 

    [Header("Sonido del Rumor")]
    public AudioClip rumorSound; // Arrastra aquí el MP3/WAV desde el Inspector
    [Range(0f, 1f)]
    public float rumorVolume = 1f; // Para controlar el volumen desde el Inspector

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
    public void TriggerRumor()
    {
        if (KnowledgeManager.Instance != null)
        {
            KnowledgeManager.Instance.LearnRumor(this);
            
            // --- REPRODUCIMOS EL SONIDO AQUÍ ---
            if (rumorSound != null)
            {
                // Hacemos que suene exactamente donde está la cámara principal 
                // para asegurarnos de que el jugador lo escucha al máximo volumen
                AudioSource.PlayClipAtPoint(rumorSound, Camera.main.transform.position, rumorVolume);
            }
        }
        else
        {
            Debug.LogWarning("Intentando aprender un rumor, pero el KnowledgeManager no está en la escena.");
        }
    }
}