using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class MenuManager : MonoBehaviour
    {
        public CanvasGroup panelFundido;
        public float velocidad = 1.5f;

        void Start()
        {
            if (panelFundido != null)
            {
                StartCoroutine(FadeIn());
            }
        }

        public void Play()
        {
            StartCoroutine(FadeOut(1)); 
        }

        IEnumerator FadeIn()
        {
            panelFundido.alpha = 1f;
            while (panelFundido.alpha > 0)
            {
                panelFundido.alpha -= Time.deltaTime * velocidad;
                yield return null;
            }
            panelFundido.blocksRaycasts = false;
        }

        IEnumerator FadeOut(int sceneIndex)
        {
            panelFundido.blocksRaycasts = true; // Bloquea clics extra por si el jugador pulsa dos veces
            while (panelFundido.alpha < 1)
            {
                panelFundido.alpha += Time.deltaTime * velocidad;
                yield return null;
            }
            SceneManager.LoadScene(sceneIndex); // Carga el juego solo cuando la pantalla ya está negra
        }
    }