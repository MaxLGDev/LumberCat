using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class RoundDefinition
{
    public RoundMechanic mechanic;
    [Tooltip("Time limit in seconds")]
    public float duration;

    [Tooltip("Taps required to clear the round")]
    public int requiredTaps;

    [Header("UI")]
    public string instructionText;
    public KeyCode[] allowedKeys;

    [TextArea]
    public string description;

    
}

public class RoundChange : MonoBehaviour
{
    [SerializeField] private Treecutting treecutting;
    [SerializeField] private TMP_Text roundCount;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Countdown Details")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private GameObject countdownCover;
    [SerializeField] private int cdTimer = 3;

    [Header("Round Details")]
    [SerializeField] private TMP_Text roundTimer;
    [SerializeField] private TMP_Text tapCounter;
    [SerializeField] private RoundDefinition[] rounds;

    public int RoundNumber { get; private set; }
    //private int maxRound = 10;
    private float currentTimer;
    private int requiredTapThisRound;
    private bool roundActive;

    private Coroutine countdownRoutine;

    [Header("Progress Bar")]
    [SerializeField] private Slider progressBar;

    private void Start()
    {
        RoundNumber = 1;

        treecutting.OnTotalTapChanged += HandleTotalTap;
        treecutting.OnRoundTapChanged += HandleRoundTap;

        roundCount.text = $"ラウンド: {RoundNumber}/{rounds.Length}";

        player.SetActive(false);
        gameOverPanel.SetActive(false);

        countdownRoutine = StartCoroutine(RoundCountdown());
    }

    private void Update()
    {
        TickRound();
    }

    private void TickRound()
    {
        if (!roundActive)
            return;

        currentTimer = Mathf.Max(0, currentTimer - Time.deltaTime);
        roundTimer.text = $"タイマー: {currentTimer:F3}";

        if (treecutting.RoundTaps >= requiredTapThisRound)
        {
            ChangeToNextRound();
        }

        if (currentTimer <= 0)
        {
            RoundLost();
        }
    }

    private void HandleRoundTap(int roundTaps)
    {
        progressBar.value = roundTaps;
    }

    private void HandleTotalTap(int totalTaps)
    {
        tapCounter.text = $"タップ: {totalTaps}";
    }

    private void SetProgress()
    {
        treecutting.ResetRound();

        progressBar.minValue = 0;
        progressBar.maxValue = requiredTapThisRound;
        progressBar.value = 0f;

        tapCounter.text = $"タップ: {treecutting.TotalTaps}";
    }

    private IEnumerator RoundCountdown()
    {
        RoundDefinition currentRound = rounds[RoundNumber - 1];

        requiredTapThisRound = currentRound.requiredTaps;
        currentTimer = currentRound.duration;

        treecutting.SetMechanic(currentRound.mechanic, requiredTapThisRound);

        SetProgress();

        roundActive = false;
        treecutting.inputEnabled = false;
        countdownCover.SetActive(true);

        for (int i = cdTimer; i > 0; i--)
        {
            countdownText.text = $"{i}...";
            yield return new WaitForSeconds(1);
        }

        countdownText.text = $"スタート!";

        yield return new WaitForSeconds(0.5f);
        roundActive = true;
        countdownCover.SetActive(false);
        player.SetActive(true);

        treecutting.inputEnabled = true;
        // start round
    }

    private void ChangeToNextRound()
    {
        roundActive = false;
        treecutting.inputEnabled = false;
        player.SetActive(false);
        
        if(countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }

        RoundNumber++;

        if (RoundNumber > rounds.Length)
        {
            Debug.Log("Game Complete!");
            return;
        }

        roundCount.text = $"ラウンド: {RoundNumber}/{rounds.Length}";
        countdownRoutine = StartCoroutine(RoundCountdown());
    }

    private void RoundLost()
    {
        treecutting.inputEnabled = false;
        gameOverPanel.SetActive(true);
        player.SetActive(false);
        Debug.Log("You lost!");
    }


}
