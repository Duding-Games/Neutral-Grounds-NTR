using UnityEngine;

public class FoodDispenser : MonoBehaviour
{
    [Tooltip("El tipo de comida que da este plato")]
    public FoodPreference foodType;
    
    [Tooltip("El Prefab 3D del plato de comida que se va a arrastrar y servir")]
    public GameObject foodPrefab;
}