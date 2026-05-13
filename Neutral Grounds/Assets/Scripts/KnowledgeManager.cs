using System.Collections.Generic;
using UnityEngine;

public class KnowledgeManager : MonoBehaviour
{
    public static KnowledgeManager Instance { get; private set; }

    [Header("Rumores Descubiertos")]
    [Tooltip("Lista de IDs de rumores que el jugador conoce")]
    public List<string> knownRumors = new List<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void LearnRumor(string rumorID)
    {
        if (!knownRumors.Contains(rumorID))
        {
            knownRumors.Add(rumorID);
            Debug.Log($"¡Nuevo rumor aprendido!: {rumorID}");
        }
    }

    public void ForgetRumor(string rumorID)
    {
        if (knownRumors.Contains(rumorID))
        {
            knownRumors.Remove(rumorID);
            Debug.Log($"Rumor gastado/olvidado: {rumorID}");
        }
    }
    public bool HasRumor(string rumorID)
    {
        return knownRumors.Contains(rumorID);
    }
}