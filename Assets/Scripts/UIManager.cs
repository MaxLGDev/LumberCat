using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;

    [Header("Links")]
    [SerializeField] private RoundManager roundManager;

    [Header("UI Details")]
    [SerializeField] private TMP_Text currentRound;
    [SerializeField] private TMP_Text totalTaps;
    [SerializeField] private TMP_Text countdown;

    [Header("Panels")]
    [SerializeField] private GameObject roundInfo;
    [SerializeField] private GameObject winResultScreen;
    [SerializeField] private GameObject loseResultScreen;

    private bool waitingForEnter = false;

    private GameManager gm;

    private Coroutine countdownCo;

    private void Start()
    {
        totalTaps.text = $"ラウンド: -";
        totalTaps.text = $"タップ: -";

        GameManager gm = FindFirstObjectByType<GameManager>();
        gm?.StartGame();
    }

    private void OnEnable()
    {
        gm = FindFirstObjectByType<GameManager>();

        if (gm != null)
        {
            gm.OnTotalTapsChanged += UpdateTotalTaps;
            gm.OnGameEnded += ShowEndScreen;
        }

        if (roundManager != null)
            roundManager.OnRoundStarted += OnRoundStarted;

        if (inputReader != null)
            inputReader.OnKeyPressed += HandleKeyPressed;
    }

    private void OnDisable()
    {
        if (gm != null)
        {
            gm.OnTotalTapsChanged -= UpdateTotalTaps;
            gm.OnGameEnded -= ShowEndScreen;
        }

        if (roundManager != null)
            roundManager.OnRoundStarted -= OnRoundStarted;

        if (inputReader != null)
            inputReader.OnKeyPressed -= HandleKeyPressed;
    }

    private void OnRoundStarted(RoundDefinition round)
    {
        roundInfo.SetActive(true);
        currentRound.text = $"ラウンド: {gm.CurrentRound} / {gm.TotalRounds}";

        waitingForEnter = true;
        countdown.text = "PRESS ENTER";

        if (countdownCo != null)
        {
            StopCoroutine(countdownCo);
            countdownCo = null;
        }

    }
    private void UpdateTotalTaps(int taps)
    {
        totalTaps.text = $"Taps: {taps}";
    }

    private IEnumerator CountdownCO()
    {
        for(int i = 3; i > 0; i--)
        {
            countdown.text = $"{i}...";
            yield return new WaitForSeconds(1);
        }

        countdown.text = "START!!!";
        yield return new WaitForSeconds(0.5f);

        roundInfo.SetActive(false);
        roundManager.StartPreparedRound();
    }

    private void ShowEndScreen(bool won)
    {
        roundInfo.SetActive(false);
        winResultScreen.SetActive(won);
        loseResultScreen.SetActive(!won);
    }

    private void HandleKeyPressed(KeyCode key)
    {
        if(waitingForEnter && key == KeyCode.Return)
        {
            waitingForEnter = false;
            countdownCo = StartCoroutine(CountdownCO());
        }
    }
}
