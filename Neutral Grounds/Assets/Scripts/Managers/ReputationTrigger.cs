using UnityEngine;

public class ReputationTrigger : MonoBehaviour
{
    public void ApplyReputation(int amount)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ModifyReputation(amount);
        }
        else
        {
            Debug.LogWarning("No se encontró el GameManager en la escena para cambiar la reputación.");
        }
    }
}