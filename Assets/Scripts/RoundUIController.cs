using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundPresentation
{
    public string instructionText;
    public string description;
}

public class RoundUIController : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;

    [Header("UI Details")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text timerDuration;

    private void Start()
    {
        timerDuration.text = $"Timer: -";
    }

    private void Update()
    {
        if (!roundManager.IsRoundActive)
            return;

        timerDuration.text = $"Timer: {roundManager.RemainingTime:F2}";
    }

    public void IncrementRoundBar()
    {
        progressBar.value += 1;
    }

    private void OnEnable()
    {
        roundManager.OnRoundStarted += InitializeRoundUI;
        roundManager.OnRoundValidInput += IncrementRoundBar;
        roundManager.OnRoundEnded += ShowRoundEnd;
    }

    private void ShowRoundEnd(bool won)
    {
        if (won)
            Debug.Log("gg");
        else
            Debug.Log("unlucky");
    }

    private void InitializeRoundUI(RoundDefinition round)
    {
        progressBar.minValue = 0;
        progressBar.maxValue = round.requiredTaps;
        progressBar.value = 0;
    }

    public void Reset()
    {
        progressBar.value = 0;
    }
}
