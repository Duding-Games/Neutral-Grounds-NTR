using UnityEngine;

[ExecuteAlways]
public class DayNightCycle : MonoBehaviour
{
    [Header("Configuración del Tiempo")]
    public float duracionSegundos = 300f;
    [Range(0f, 1f)] public float progresoDia = 0f;
    public bool elTiempoPasaSolo = true;

    [Header("Ajustes del Sol")]
    public Gradient colorDelSol; // Configura esto en el Inspector (Naranja -> Blanco -> Naranja)
    public float intensidadMaxima = 1.3f;
    public float intensidadMinima = 0f;

    [Header("Valores de Atmosphere")]
    public float grosorInicio = 0.6f;
    public float grosorFin = 4f;

    [Header("Referencias")]
    public Light sol;
    public Material skyboxMaterial; // El de image_6b93c9.png

    void Update()
    {
        if (Application.isPlaying && elTiempoPasaSolo)
        {
            progresoDia += Time.deltaTime / duracionSegundos;
            if (progresoDia >= 1f) progresoDia = 0f;
        }

        ActualizarIluminacion();
    }

    void OnValidate() => ActualizarIluminacion();

    void ActualizarIluminacion()
    {
        if (sol == null || skyboxMaterial == null) return;

        // 1. Rotación (Ciclo natural)
        sol.transform.localRotation = Quaternion.Euler(progresoDia * 180, 170f, 0f);

        // 2. Intensidad y Color dinámico
        // Usamos el Dot product para saber si el sol mira hacia abajo (día) o arriba (noche)
        float factorPunto = Mathf.Max(0, Vector3.Dot(sol.transform.forward, Vector3.down));
        
        sol.intensity = Mathf.Lerp(intensidadMinima, intensidadMaxima, factorPunto);
        sol.color = colorDelSol.Evaluate(progresoDia);

        // 3. Atmosphere Thickness (lo que ya tenías)
        float grosorActual = Mathf.Lerp(grosorInicio, grosorFin, progresoDia);
        skyboxMaterial.SetFloat("_AtmosphereThickness", grosorActual);

        // 4. EL TRUCO: Actualizar la luz ambiental
        // Esto hace que los objetos pillen el color de la skybox en cada frame
        DynamicGI.UpdateEnvironment();
    }
}