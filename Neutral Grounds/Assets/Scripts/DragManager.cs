using UnityEngine;

public class DragManager : MonoBehaviour
{
    private Camera _mainCamera;

    [Header("Configuración de Detección")]
    public string draggableTag = "Draggable"; // Tag que deben tener los objetos
    public LayerMask esceneLayers; // Opcional: Capas que el Raycast debe ignorar (ej: triggers)

    [Header("Configuración Visual (Hover)")]
    public Color hoverColor = Color.yellow;
    
    // Variables para gestionar el hover actual
    private Renderer _currentHoveredRenderer;
    private Color _initialHoveredColor;

    [Header("Configuración de Arrastre")]
    public float liftHeight = 0.3f; // Cuánto sube el objeto al agarrarlo

    // Variables para gestionar el arrastre actual
    private Rigidbody _selectedRb;
    private float _targetY; // La altura fija a la que flotará el objeto
    private Plane _movementPlane; // Plano matemático horizontal (XZ)
    private Vector3 _dragOffset; // Distancia entre el centro del objeto y donde pinchó el ratón

    void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null) Debug.LogError("No hay una cámara principal en la escena.");
    }

    void Update()
    {
        // 1. Gestión del Hover (Color amarillo)
        HandleHover();

        // 2. Gestión de Inputs (Clic, Arrastrar, Soltar)
        HandleInput();
    }

    // --- LÓGICA DEL HOVER (Cambio de Color) ---
    private void HandleHover()
    {
        // Si estamos arrastrando algo, no calculamos hover en otros objetos
        if (_selectedRb != null)
        {
            ClearHover();
            return;
        }

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Lanzamos un rayo constante desde la cámara al ratón
        if (Physics.Raycast(ray, out hit, float.PositiveInfinity))
        {
            // Si tocamos algo con el Tag correcto
            if (hit.collider.CompareTag(draggableTag))
            {
                Renderer newRenderer = hit.collider.GetComponent<Renderer>();
                
                if (newRenderer != null)
                {
                    // Si es un objeto DISTINTO al que ya estábamos iluminando
                    if (newRenderer != _currentHoveredRenderer)
                    {
                        ClearHover(); // Restauramos el color del anterior

                        // Guardamos y cambiamos el color del nuevo
                        _currentHoveredRenderer = newRenderer;
                        _initialHoveredColor = _currentHoveredRenderer.material.color;
                        _currentHoveredRenderer.material.color = hoverColor;
                    }
                    // Si es el mismo, no hacemos nada (ya está amarillo)
                    return; 
                }
            }
        }

        // Si el rayo no toca nada o toca algo sin el tag, limpiamos el hover actual
        ClearHover();
    }

    // Restaura el color original del objeto que tenía el hover
    private void ClearHover()
    {
        if (_currentHoveredRenderer != null)
        {
            _currentHoveredRenderer.material.color = _initialHoveredColor;
            _currentHoveredRenderer = null;
        }
    }


    // --- LÓGICA DEL INPUT Y MOVIMIENTO ---
    private void HandleInput()
    {
        // Botón izquierdo presionado: Intentar agarrar
        if (Input.GetMouseButtonDown(0))
        {
            TryPickUp();
        }

        // Botón izquierdo soltado: Soltar objeto
        if (Input.GetMouseButtonUp(0) && _selectedRb != null)
        {
            DropObject();
        }
    }

    // Usamos FixedUpdate para mover objetos físicos para mayor suavidad
    void FixedUpdate()
    {
        if (_selectedRb != null)
        {
            DragObject();
        }
    }

    private void TryPickUp()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag(draggableTag))
            {
                _selectedRb = hit.collider.attachedRigidbody;

                if (_selectedRb != null)
                {
                    // Al agarrar, quitamos el hover visual (opcional)
                    ClearHover();

                    // Configuramos físicas
                    _selectedRb.useGravity = false;
                    _selectedRb.isKinematic = true; // O false si quieres colisiones físicas reales al mover

                    // Calculamos la altura de arrastre (su Y actual + el levantamiento)
                    _targetY = _selectedRb.position.y + liftHeight;

                    // Creamos el PLANO MATEMÁTICO. 
                    // Es un plano horizontal (mirando hacia arriba Vector3.up) 
                    // situado a la altura objetivo (_targetY).
                    _movementPlane = new Plane(Vector3.up, new Vector3(0, _targetY, 0));

                    // Calculamos dónde toca el rayo del ratón en ese plano matemático
                    float distance;
                    if (_movementPlane.Raycast(ray, out distance))
                    {
                        Vector3 pointOnPlane = ray.GetPoint(distance);
                        
                        // Calculamos el offset (diferencia) para que el objeto no "salte" al centro del puntero
                        // Proyectamos la posición actual del objeto a la altura de arrastre para el cálculo
                        Vector3 objProjectedPos = _selectedRb.position;
                        objProjectedPos.y = _targetY;
                        _dragOffset = objProjectedPos - pointOnPlane;
                    }
                }
            }
        }
    }

    private void DragObject()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        float distance;

        // Buscamos continuamente dónde apunta el ratón en el plano matemático horizontal
        if (_movementPlane.Raycast(ray, out distance))
        {
            Vector3 currentPointOnPlane = ray.GetPoint(distance);
            
            // La posición destino es el punto en el plano + el offset inicial,
            // asegurando que mantenemos la Y fija.
            Vector3 targetPosition = currentPointOnPlane + _dragOffset;
            targetPosition.y = _targetY; 

            // Movemos el Rigidbody (mejor que transform.position para físicas)
            _selectedRb.MovePosition(targetPosition);
        }
    }

    private void DropObject()
    {
        // Restauramos físicas
        _selectedRb.useGravity = true;
        _selectedRb.isKinematic = false;
        _selectedRb = null;
    }
}