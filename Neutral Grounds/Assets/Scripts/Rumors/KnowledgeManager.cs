using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class KnowledgeManager : MonoBehaviour
{
    public static KnowledgeManager Instance { get; private set; }

    [Header("Rumores Conocidos")]
    public List<RumorData> knownRumors = new List<RumorData>();

    [Header("Feedback Visual (Notificación)")]
    public GameObject notificationPanel; 
    public TextMeshProUGUI notificationText;

    [Header("Menú de Rumores (UI)")]
    public GameObject rumorMenuPanel; 
    public Transform buttonContainer; 
    public GameObject rumorButtonPrefab; 

    public bool isMenuOpen = false; // El candado de seguridad
    private NPCController currentTargetNPC;
    private int frameWhenOpened; // Para evitar que se cierre en el mismo frame que se abre

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (notificationPanel != null) notificationPanel.SetActive(false);
        if (rumorMenuPanel != null) rumorMenuPanel.SetActive(false); 
    }

    private void Update()
    {
        // SISTEMA DE EMERGENCIA: Si el menú está abierto, podemos forzar el cierre con ESCAPE o Click Derecho
        if (isMenuOpen && Time.frameCount > frameWhenOpened)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                Debug.Log("[KnowledgeManager] Cierre de emergencia activado por el jugador.");
                CloseRumorMenu();
            }
        }
    }

    public void LearnRumor(RumorData newRumor)
    {
        if (!knownRumors.Contains(newRumor))
        {
            knownRumors.Add(newRumor);
            Debug.Log("Rumor aprendido: " + newRumor.rumorName);
            StartCoroutine(ShowNotificationRoutine("¡Nuevo Rumor Aprendido!\n" + newRumor.rumorName));
        }
    }

    private IEnumerator ShowNotificationRoutine(string message)
    {
        if (notificationPanel != null && notificationText != null)
        {
            notificationText.text = message;
            notificationPanel.SetActive(true); 
            yield return new WaitForSecondsRealtime(4f); 
            
            // IMPORTANTE: Solo apagamos el panel de notificaciones
            notificationPanel.SetActive(false); 
        }
    }

    // --- LÓGICA DEL MENÚ CON PROTECCIÓN ANTIFALLOS ---
    
    public void OpenRumorMenu(NPCController target)
    {
        Debug.Log("[KnowledgeManager] Intentando abrir el menú de rumores...");

        if (knownRumors.Count == 0)
        {
            Debug.LogWarning("[KnowledgeManager] Cancelado: No sabes ningún rumor todavía.");
            return; 
        }

        if (rumorMenuPanel == null || buttonContainer == null || rumorButtonPrefab == null)
        {
            Debug.LogError("[KnowledgeManager] ERROR GRAVE: Te has olvidado de asignar el Panel, el Contenedor o el Prefab del botón en el Inspector.");
            return;
        }

        try
        {
            currentTargetNPC = target;
            isMenuOpen = true; 
            frameWhenOpened = Time.frameCount; // Guardamos en qué frame se abrió
            
            rumorMenuPanel.SetActive(true);
            Debug.Log("[KnowledgeManager] Panel activado. Limpiando botones antiguos...");

            // 1. Limpiar botones antiguos
            foreach (Transform child in buttonContainer)
            {
                Destroy(child.gameObject);
            }

            Debug.Log($"[KnowledgeManager] Creando {knownRumors.Count} botones...");

            // 2. Crear los nuevos botones
            foreach (RumorData rumor in knownRumors)
            {
                GameObject newBtnObj = Instantiate(rumorButtonPrefab, buttonContainer);
                
                TextMeshProUGUI btnText = newBtnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) 
                {
                    btnText.text = rumor.rumorName; 
                }
                else
                {
                    Debug.LogWarning("[KnowledgeManager] OJO: Tu Prefab de botón no tiene un Texto de TextMeshPro dentro.");
                }

                Button btn = newBtnObj.GetComponent<Button>();
                if (btn != null)
                {
                    RumorData rumorAsignado = rumor; 
                    btn.onClick.AddListener(() => OnRumorSelected(rumorAsignado));
                }
                else
                {
                    Debug.LogWarning("[KnowledgeManager] OJO: Tu Prefab de botón no tiene el componente 'Button'.");
                }
            }

            Debug.Log("[KnowledgeManager] Menú abierto con éxito.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[KnowledgeManager] EL CÓDIGO HA CRASHEADO AL ABRIR EL MENÚ: " + e.Message);
        }
    }

    public void CloseRumorMenu()
    {
        Debug.Log("[KnowledgeManager] Apagando panel del menú...");
        if (rumorMenuPanel != null) rumorMenuPanel.SetActive(false);
        currentTargetNPC = null;
        
        StartCoroutine(UnlockMenuRoutine());
    }

    private IEnumerator UnlockMenuRoutine()
    {
        // Usamos Realtime para que quite el candado incluso si un diálogo congeló el tiempo (Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(0.1f); 
        isMenuOpen = false;
        Debug.Log("[KnowledgeManager] Candado quitado. Ya puedes hacer click en el 3D.");
    }

    private void OnRumorSelected(RumorData rumor)
    {
        Debug.Log($"[KnowledgeManager] Has hecho click en el rumor: {rumor.rumorName}");
        if (currentTargetNPC != null)
        {
            currentTargetNPC.ReceiveRumor(rumor);
        }
        CloseRumorMenu();
    }
}