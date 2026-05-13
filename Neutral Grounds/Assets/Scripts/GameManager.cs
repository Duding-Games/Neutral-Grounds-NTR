using UnityEngine;
using UnityEngine.UI; 
using TMPro; 
using System.Collections;
using Yarn.Unity;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MorningBriefing, TavernOpen, ClosingTime, NightTransition, GameOver }

    [Header("Script References")]
    public DayNightCycle dayNightScript;

    [Header("Yarn Spinner")]
    public DialogueRunner dialogueRunner;
    public InMemoryVariableStorage variableStorage;
    
    [Header("Game State (Read Only)")]
    public GameState currentState;
    public int currentDay = 1;
    public int activeCustomerCount = 0;

    [Header("Settings")]
    public int maxDays = 3;
    [Tooltip("Duration of the day in seconds (e.g., 300 for 5 mins)")]
    public float dayDurationSeconds = 30f; 
    private float dayTimer;

    [Header("UI References")]
    [Tooltip("A Canvas Group attached to a black Panel covering the screen")]
    public CanvasGroup transitionScreen; 
    [Tooltip("A text element to show 'Day 1', etc.")]
    public TextMeshProUGUI transitionText; 
    public float fadeSpeed = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(StartDaySequence());
    }

    private void Update()
    {
        if (currentState == GameState.TavernOpen)
        {
            dayTimer -= Time.deltaTime;

            if (dayNightScript != null)
            {
                dayNightScript.progresoDia = 1f - (dayTimer / dayDurationSeconds);
            }
            
            if (dayTimer <= 0)
            {
                Debug.Log("Time is up! Doors are locked. Waiting for remaining customers...");
                currentState = GameState.ClosingTime;
            }
        }
        else if (currentState == GameState.ClosingTime)
        {
            if (activeCustomerCount <= 0)
            {
                Debug.Log("Tavern is empty. Ending the day.");
                EndDay();
            }
        }
    }

    public void RegisterCustomer() { activeCustomerCount++; }
    public void UnregisterCustomer() { activeCustomerCount--; }

    private IEnumerator StartDaySequence()
    {
        currentState = GameState.MorningBriefing;
        
        if (transitionText != null) transitionText.text = "Day " + currentDay;

        yield return FadeScreen(1f); 

        if (dayNightScript != null) 
        {
            dayNightScript.progresoDia = 0f;
        }

        Debug.Log($"Playing radio update for Day {currentDay}...");

        if (dialogueRunner != null)
        {
            variableStorage.SetValue("$currentDay", currentDay);
            dialogueRunner.StartDialogue("RadioMorning");
            yield return null;

            while (dialogueRunner.IsDialogueRunning)
            {
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        dayTimer = dayDurationSeconds;
        currentState = GameState.TavernOpen;
        Debug.Log($"Tavern is OPEN! Day {currentDay} has begun.");

        yield return FadeScreen(0f); 
    }

    private void EndDay()
    {
        currentState = GameState.NightTransition;
        currentDay++;

        if (currentDay > maxDays)
        {
            Debug.Log("Game Over! Calculate final scores and show endings.");
            currentState = GameState.GameOver;
        }
        else
        {
            StartCoroutine(StartDaySequence());
        }
    }
    public void ActualizarDatosParaRadio()
    {

        int tension = 15;
        float ganancias = 120.5f;
        bool huboPelea = true;

        variableStorage.SetValue("$tension", tension);
        variableStorage.SetValue("$dinero_total", ganancias);
        variableStorage.SetValue("$hubo_pelea", huboPelea);
    }

    private IEnumerator FadeScreen(float targetAlpha)
    {
        if (transitionScreen == null) yield break;

        while (Mathf.Abs(transitionScreen.alpha - targetAlpha) > 0.01f)
        {
            transitionScreen.alpha = Mathf.MoveTowards(transitionScreen.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }
        transitionScreen.alpha = targetAlpha;
    }
}