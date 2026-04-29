using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Draggable : MonoBehaviour
{
    private Camera _mainCamera;
    private Rigidbody _rb;
    private Vector3 _screenPoint;
    private Vector3 _offset;
    
    [Header("Settings")]
    public Color hoverColor = Color.yellow;
    private Color _initialColor;
    private Renderer _renderer;

    void Start()
    {
        _mainCamera = Camera.main;
        _rb = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        
        if (_renderer != null)
            _initialColor = _renderer.material.color;
    }

    // Visual feedback when hovering mouse
    void OnMouseEnter()
    {
        if (_renderer != null)
            _renderer.material.color = hoverColor;
    }

    void OnMouseExit()
    {
        if (_renderer != null)
            _renderer.material.color = _initialColor;
    }

    void OnMouseDown()
    {
        // Disable gravity while dragging so it doesn't feel heavy
        _rb.useGravity = false;
        _rb.isKinematic = true; 

        _screenPoint = _mainCamera.WorldToScreenPoint(gameObject.transform.position);
        _offset = gameObject.transform.position - GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos() + _offset;
    }

    void OnMouseUp()
    {
        // Re-enable physics when dropping
        _rb.useGravity = true;
        _rb.isKinematic = false;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = _screenPoint.z; // Maintain object's depth
        return _mainCamera.ScreenToWorldPoint(mousePoint);
    }
}