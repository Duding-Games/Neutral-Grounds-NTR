using UnityEngine;
using Yarn.Unity; // Súper importante añadir esto

public class RadioSound : MonoBehaviour
{
    [SerializeField] private AudioSource sonidoRadio;

    // Con esta etiqueta creamos el comando para Yarn
    [YarnCommand("empezar_radio")]
    public void EmpezarRadio()
    {
        if (sonidoRadio != null && !sonidoRadio.isPlaying)
        {
            sonidoRadio.Play();
        }
    }

    [YarnCommand("parar_radio")]
    public void PararRadio()
    {
        if (sonidoRadio != null)
        {
            sonidoRadio.Stop();
        }
    }
}