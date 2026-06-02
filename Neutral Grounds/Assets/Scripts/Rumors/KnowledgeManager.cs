using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // <-- Necesario para usar TextMeshPro

public class KnowledgeManager : MonoBehaviour
{
    public static KnowledgeManager Instance { get; private set; }

    [Header("Rumores Conocidos")]
    public List<RumorData> knownRumors = new List<RumorData>();

    [Header("Feedback Visual (UI)")]
    public GameObject notificationPanel; // Un panel oscuro de fondo
    public TextMeshProUGUI notificationText; // <-- Cambiado a TextMeshProUGUI

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

    // Llama a esto desde un Unity Event en tu Dialogue Editor
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
            notificationPanel.SetActive(true); // Encendemos el cartel
            
            yield return new WaitForSeconds(4f); // Esperamos 4 segundos
            
            notificationPanel.SetActive(false); // Lo apagamos
        }
    }
}