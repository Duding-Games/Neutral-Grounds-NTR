using UnityEngine;

public class DragManager : MonoBehaviour
{
    private Camera _mainCamera;

    [Header("Configuración de Interacción")]
    public string dispenserTag = "FoodDispenser"; 
    public LayerMask chairLayer; 
    public float liftHeight = 1.5f; 

    [Header("Configuración Visual de la Línea")]
    public Material lineMaterial; // Opcional: Arrastra un material básico aquí en el Inspector
    public float lineWidth = 0.05f;

    // Variables de estado interno
    private GameObject _currentDraggedFood;
    private FoodPreference _currentFoodType;
    private float _targetY;
    private Plane _movementPlane;
    private LineRenderer _dropLine;

    void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null) Debug.LogError("No hay una cámara principal en la escena.");

        // Configuramos la línea visual por código para no tener que añadirla a mano
        _dropLine = gameObject.AddComponent<LineRenderer>();
        _dropLine.startWidth = lineWidth;
        _dropLine.endWidth = lineWidth;
        _dropLine.positionCount = 2;
        _dropLine.enabled = false;
        
        // Si no asignas un material, le pone el por defecto para que se vea blanca
        if (lineMaterial != null) _dropLine.material = lineMaterial;
        else _dropLine.material = new Material(Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TryGrabFood();

        if (_currentDraggedFood != null && Input.GetMouseButton(0)) DragFood();

        if (Input.GetMouseButtonUp(0) && _currentDraggedFood != null) TryServeFood();
    }

    private void TryGrabFood()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag(dispenserTag))
            {
                FoodDispenser dispenser = hit.collider.GetComponent<FoodDispenser>();
                if (dispenser != null && dispenser.foodPrefab != null)
                {
                    _currentDraggedFood = Instantiate(dispenser.foodPrefab);
                    _currentFoodType = dispenser.foodType;

                    Collider col = _currentDraggedFood.GetComponent<Collider>();
                    if (col != null) col.enabled = false;

                    _targetY = hit.point.y + liftHeight;
                    _movementPlane = new Plane(Vector3.up, new Vector3(0, _targetY, 0));
                }
            }
        }
    }

    private void DragFood()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (_movementPlane.Raycast(ray, out float distance))
        {
            Vector3 pointOnPlane = ray.GetPoint(distance);
            _currentDraggedFood.transform.position = pointOnPlane;
            
            // --- LÓGICA DE LA LÍNEA VISUAL ---
            _dropLine.enabled = true;
            _dropLine.SetPosition(0, _currentDraggedFood.transform.position);

            // Lanzamos un rayo hacia abajo desde la comida para ver qué hay debajo
            if (Physics.Raycast(_currentDraggedFood.transform.position, Vector3.down, out RaycastHit hitDown, Mathf.Infinity))
            {
                _dropLine.SetPosition(1, hitDown.point);

                // Si lo que hay debajo es de la capa ChairLayer, la línea se pone verde
                if (((1 << hitDown.collider.gameObject.layer) & chairLayer) != 0)
                {
                    _dropLine.startColor = Color.green;
                    _dropLine.endColor = Color.green;
                }
                else
                {
                    _dropLine.startColor = Color.red;
                    _dropLine.endColor = Color.red;
                }
            }
        }
    }

    private void TryServeFood()
    {
        _dropLine.enabled = false; // Apagamos la línea al soltar
        bool successfullyServed = false;

        // ¡EL CAMBIO CLAVE! En vez de usar la posición del ratón, comprobamos qué hay justo debajo de la comida
        if (Physics.Raycast(_currentDraggedFood.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, chairLayer))
        {
            Collider[] colliders = Physics.OverlapSphere(hit.collider.transform.position, 1.5f);
            
            foreach (Collider col in colliders)
            {
                NPCController npc = col.GetComponent<NPCController>();
                
                if (npc != null && npc.currentState == NPCController.NPCState.WaitingForFood)
                {
                    npc.ReceiveOrder(_currentFoodType);

                    Collider foodCol = _currentDraggedFood.GetComponent<Collider>();
                    if (foodCol != null) foodCol.enabled = true;

                    Transform anchor = hit.collider.transform.Find("PlateAnchor");

                    if (anchor != null)
                    {
                        _currentDraggedFood.transform.position = anchor.position;
                        _currentDraggedFood.transform.rotation = anchor.rotation;
                    }
                    else
                    {
                        _currentDraggedFood.transform.position = hit.collider.transform.position + Vector3.up * 1f;
                        Debug.LogWarning("¡A esta silla le falta el PlateAnchor!");
                    }

                    successfullyServed = true;
                    break;
                }
            }
        }

        if (!successfullyServed)
        {
            Destroy(_currentDraggedFood);
        }

        _currentDraggedFood = null;
    }
}