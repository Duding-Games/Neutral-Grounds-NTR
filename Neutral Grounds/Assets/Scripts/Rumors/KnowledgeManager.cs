using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class KnowledgeManager : MonoBehaviour
{
    public static KnowledgeManager Instance { get; private set; }

    [Header("Rumores Conocidos")]
    public List<RumorData> knownRumors = new List<RumorData>();

    [Header("Feedback Visual (Notificación)")]
    public GameObject notificationPanel; 
    public TextMeshProUGUI notificationText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Apagamos el panel al empezar el juego
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    public void LearnRumor(RumorData newRumor)
    {
        if (!knownRumors.Contains(newRumor))
        {
            knownRumors.Add(newRumor);
            Debug.Log("Rumor aprendido: " + newRumor.rumorName);
            
            // Activamos el feedback visual
            StartCoroutine(ShowNotificationRoutine("¡Nuevo Rumor Aprendido!\n" + newRumor.rumorName));
        }
    }

    private IEnumerator ShowNotificationRoutine(string message)
    {
        if (notificationPanel != null && notificationText != null)
        {
            notificationText.text = message;
            notificationPanel.SetActive(true); 
            
            // Usamos Realtime para que no se congele si el diálogo pausa el juego
            yield return new WaitForSecondsRealtime(4f); 
            
            notificationPanel.SetActive(false); 
        }
    }
}