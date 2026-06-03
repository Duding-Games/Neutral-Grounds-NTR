using UnityEngine;

public class DoorSound : MonoBehaviour
{
    [SerializeField] private AudioSource miSonido;

    private void OnTriggerEnter(Collider other)
    {
        // Si quieres que solo suene con los clientes: 
        // if (other.CompareTag("Cliente"))
        
        miSonido.Play();
    }
}