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
    private enum GameState
    {
        Countdown,
        WaitingForStart,
        InRound,
        GameOver
    }

    private GameState state;

    [SerializeField] private Treecutting[] treecutting;
    [SerializeField] private TMP_Text roundCount;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Countdown Details")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private GameObject countdownCover;
    [SerializeField] private TMP_Text timeLimitInfoText;
    [SerializeField] private TMP_Text requiredButtonsText;
    [SerializeField] private int cdTimer = 3;

    [Header("Round Details")]
    [SerializeField] private TMP_Text roundTimer;
    [SerializeField] private TMP_Text tapCounter;
    [SerializeField] private RoundDefinition[] rounds;

    
    private int totalTaps;
    private int roundTaps;

    public int RoundNumber { get; private set; }
    //private int maxRound = 10;
    private float currentTimer;
    private int requiredTapThisRound;

    [Header("Progress Bar")]
    [SerializeField] private Slider progressBar;

    private void Start()
    {
        RoundNumber = 1;
        PrepareRound();

        foreach (var t in treecutting)
        {
            t.OnValidTap += HandleValidTap;
            t.OnInvalidTap += HandleInvalidTap;
            t.inputEnabled = false;
        }

        roundCount.text = $"ラウンド: {RoundNumber}/{rounds.Length}";
        player.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        foreach (var t in treecutting)
        {
            t.OnValidTap -= HandleValidTap;
        }
    }

    private void HandleValidTap(int value)
    {
        roundTaps++;
        totalTaps = roundTaps;

        tapCounter.text = $"タップ: {totalTaps}";
        progressBar.value = roundTaps;

        if (roundTaps >= requiredTapThisRound)
            ChangeToNextRound();
    }

    private void HandleInvalidTap(int value)
    {
        totalTaps++;
    }

    private void Update()
    {
        switch(state)
        {
            case GameState.WaitingForStart:
                if (Input.GetKeyDown(KeyCode.Return))
                    StartRound();
                break;

            case GameState.InRound:
                TickRound();
                break;
        }
    }

    private void TickRound()
    {
        if (currentTimer > 0f)
        {
            currentTimer -= Time.deltaTime;
            roundTimer.text = $"タイマー: {currentTimer:F3}";

            if (currentTimer <= 0)
            {
                RoundLost();
                return;
            }
        }
        else
        {
            roundTimer.text = "Timer: ∞";
            currentTimer = -1f;
        }
    }

    private void SetProgress()
    {
        foreach (var t in treecutting)
            t.ResetRound();

        progressBar.minValue = 0;
        progressBar.maxValue = requiredTapThisRound;
        progressBar.value = 0f;

        tapCounter.text = $"タップ: {totalTaps}";
    }

    private void PrepareRound()
    {
        RoundDefinition currentRound = rounds[RoundNumber - 1];

        requiredTapThisRound = currentRound.requiredTaps;
        currentTimer = currentRound.duration;

        roundTaps = 0;
        progressBar.value = 0;

        foreach (var t in treecutting)
            t.SetMechanic(currentRound.mechanic, requiredTapThisRound);

        SetProgress();

        player.SetActive(false);

        roundTimer.text = "Timer: ";

        countdownCover.SetActive(true);
        countdownText.text = "Press 'Enter' to start";

        if (currentRound.duration <= 0)
            timeLimitInfoText.text = $"Time: ∞";
        else
            timeLimitInfoText.text = $"Time: {currentRound.duration}s";

        requiredButtonsText.text = $"Buttons: \n" + string.Join(" / ", currentRound.allowedKeys);

        state = GameState.WaitingForStart;
    }

    private void StartRound()
    {
        if (state != GameState.WaitingForStart)
            return;

        StartCoroutine(StartRoundCO());
    }

    private IEnumerator StartRoundCO()
    {
        state = GameState.Countdown;

        for (int i = cdTimer; i > 0; i--)
        {
            countdownText.text = $"{i}...";
            yield return new WaitForSeconds(1);
        }

        countdownText.text = $"スタート!";
        yield return new WaitForSeconds(0.5f);

        countdownCover.SetActive(false);
        player.SetActive(true);

        foreach (var t in treecutting)
            t.inputEnabled = true;

        state = GameState.InRound;
    }

    private void ChangeToNextRound()
    {
        foreach (var t in treecutting)
            t.inputEnabled = false;

        player.SetActive(false);
        
        RoundNumber++;

        if (RoundNumber > rounds.Length)
        {
            state = GameState.GameOver;
            Debug.Log("Game Complete!");
            return;
        }

        roundCount.text = $"ラウンド: {RoundNumber}/{rounds.Length}";
        PrepareRound();
    }

    private void RoundLost()
    {
        state = GameState.GameOver;

        foreach (var t in treecutting)
            t.inputEnabled = false;

        gameOverPanel.SetActive(true);
        player.SetActive(false);
        Debug.Log("You lost!");
    }


}
