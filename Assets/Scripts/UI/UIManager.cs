using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Localization;

public class UIManager : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;

    [Header("Links")]
    [SerializeField] private RoundManager roundManager;

    [Header("Game UI Elements")]
    [SerializeField] private TMP_Text currentRound;
    [SerializeField] private TMP_Text totalTaps;
    [SerializeField] private TMP_Text countdown;
    [SerializeField] private GameObject keySlots;

    [Header("Localization - In Game")]
    [SerializeField] private LocalizedString pressedEnterString;
    [SerializeField] private LocalizedString countdownString;
    [SerializeField] private LocalizedString startString;
    [SerializeField] private LocalizedString roundString;
    [SerializeField] private LocalizedString tapsString;

    private bool waitingForEnter = false;
    private GameManager gm;
    private Coroutine countdownCo;

    private void Start()
    {
        keySlots.SetActive(false);
        
        roundString.Arguments = new object[] { "-" };
        currentRound.text = roundString.GetLocalizedString();

        tapsString.Arguments = new object[] { "-" };
        totalTaps.text = tapsString.GetLocalizedString();

        gm = FindFirstObjectByType<GameManager>();
        gm?.StartGame();
    }

    private void OnEnable()
    {
        gm = FindFirstObjectByType<GameManager>();

        if (gm != null)
        {
            gm.OnTotalTapsChanged += UpdateTotalTaps;
            gm.OnGameEnded += HandleGameEnded;
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
            gm.OnGameEnded -= HandleGameEnded;
        }

        if (roundManager != null)
            roundManager.OnRoundStarted -= OnRoundStarted;

        if (inputReader != null)
            inputReader.OnKeyPressed -= HandleKeyPressed;
    }

    private void OnRoundStarted()
    {
        PanelManager.Instance.ShowRoundTransition(true);

        roundString.Arguments = new object[] { gm.CurrentRound };
        currentRound.text = roundString.GetLocalizedString();

        waitingForEnter = true;
        countdown.text = pressedEnterString.GetLocalizedString();

        if (countdownCo != null)
        {
            StopCoroutine(countdownCo);
            countdownCo = null;
        }
    }

    private void UpdateTotalTaps(int taps)
    {
        tapsString.Arguments = new object[] { taps };
        totalTaps.text = tapsString.GetLocalizedString();
    }

    private IEnumerator CountdownCO()
    {
        for (int i = 3; i > 0; i--)
        {
            countdownString.Arguments = new object[] { i };
            countdown.text = countdownString.GetLocalizedString();
            yield return new WaitForSeconds(1);
        }

        countdown.text = startString.GetLocalizedString();
        yield return new WaitForSeconds(0.5f);

        PanelManager.Instance.ShowRoundTransition(false);
        keySlots.SetActive(true);
        roundManager.StartPreparedRound();
    }

    private void HandleGameEnded(bool won)
    {
        keySlots.SetActive(false);
        PanelManager.Instance.ShowEndScreen(won);
        PanelManager.Instance.ShowFinalTaps();
    }

    private void HandleKeyPressed(KeyCode key)
    {
        if (waitingForEnter && key == KeyCode.Return)
        {
            waitingForEnter = false;
            countdownCo = StartCoroutine(CountdownCO());
        }
    }
}