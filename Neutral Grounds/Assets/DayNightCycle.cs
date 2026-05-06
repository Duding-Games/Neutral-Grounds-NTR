using UnityEngine;

public class DayNightBoxophobic : MonoBehaviour
{
    [Header("Referencias")]
    public Light sunLight;
    public Material skyboxMaterial;

    [Header("Configuración")]
    public float dayDurationSeconds = 60f;
    [Range(0, 1)] public float currentTime = 0.25f;

    void Update()
    {
        // 1. Lógica de tiempo
        currentTime += Time.deltaTime / dayDurationSeconds;
        if (currentTime >= 1) currentTime = 0;

        // 2. Rotación física del sol (esto ya te funcionaba)
        float sunRotation = currentTime * 360f - 90f;
        sunLight.transform.rotation = Quaternion.Euler(sunRotation, 170f, 0f);

        // 3. Lógica para el Material de Boxophobic
        if (skyboxMaterial != null)
        {
            // Calculamos qué tan "de día" es (1 = mediodía, 0 = noche)
            float dayFactor = Mathf.Clamp01(Vector3.Dot(sunLight.transform.forward, Vector3.down));

            // OPCIÓN A: Si usas dos Cubemaps (Día y Noche)
            // Cambia "_CubemapTransition" por el nombre que salga en tu shader
            skyboxMaterial.SetFloat("_CubemapTransition", 1 - dayFactor);

            // OPCIÓN B: Ajustar la exposición (para que se oscurezca de noche)
            // Ajusta los valores 0.2f (noche) y 1.2f (día) a tu gusto
            float exposure = Mathf.Lerp(0.2f, 1.2f, dayFactor);
            skyboxMaterial.SetFloat("_Exposure", exposure);
            
            // OPCIÓN C: Cambiar el tinte (opcional)
            Color nightColor = new Color(0.1f, 0.1f, 0.2f);
            Color dayColor = Color.white;
            skyboxMaterial.SetColor("_Tint", Color.Lerp(nightColor, dayColor, dayFactor));
        }

        // Ajustar intensidad de la luz para que no ilumine desde el suelo
        sunLight.intensity = Mathf.Lerp(0, 1, Mathf.Clamp01(Vector3.Dot(sunLight.transform.forward, Vector3.down) * 5));
    }
}