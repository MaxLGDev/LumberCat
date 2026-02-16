using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Game UI Elements")]
    [SerializeField] private TMP_Text currentRound;
    [SerializeField] private TMP_Text totalTaps;
    [SerializeField] private TMP_Text countdown;
    [SerializeField] private GameObject keySlots;
    [SerializeField] private TMP_Text roundClearedText;
    [SerializeField] private TMP_Text complimentText;

    [Header("Localization - In Game")]
    [SerializeField] private LocalizedString pressedEnterString;
    [SerializeField] private LocalizedString countdownString;
    [SerializeField] private LocalizedString startString;
    [SerializeField] private LocalizedString roundString;
    [SerializeField] private LocalizedString tapsString;
    [SerializeField] private LocalizedString firstRoundString;
    [SerializeField] private LocalizedString warmUpString;
    [SerializeField] private LocalizedString roundClearedString;
    [SerializeField] private LocalizedString[] complimentStrings;

    [Header("Web Links")]
    [SerializeField] private string unityroomUrl = "https://unityroom.com/users/maxlgdev";
    [SerializeField] private string githubUrl = "https://github.com/MaxLGDev";

    private bool enterPressed;

    private GameManager gm;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Only one UI Manager can exist!");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        gm = GameManager.Instance;

        roundString.StringChanged += value =>
        {
            currentRound.text = value;
        };

        // TAPS TEXT
        tapsString.StringChanged += value =>
        {
            totalTaps.text = value;
        };
    }

    private void Start()
    {
        keySlots.SetActive(false);

        PanelManager.Instance.ShowMainMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && gm.CurrentState == GameState.RoundTransition)
            enterPressed = true;
    }

    private void OnEnable()
    {
        if (gm == null)
            return;

        gm.OnTotalTapsChanged += UpdateTotalTaps;
        gm.OnGameEnded += HandleGameEnded;
        gm.OnGameStateChanged += HandleGameStateChanged;
        gm.OnRoundChanged += UpdateRoundNumber;

        HandleGameStateChanged(gm.CurrentState);
        UpdateRoundNumber();
        UpdateTotalTaps(gm.TotalTaps);
    }

    private void OnDisable()
    {
        if (gm != null)
        {
            gm.OnTotalTapsChanged -= UpdateTotalTaps;
            gm.OnGameEnded -= HandleGameEnded;
            gm.OnGameStateChanged -= HandleGameStateChanged;
            gm.OnRoundChanged -= UpdateRoundNumber;
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.RoundTransition:
                PanelManager.Instance.ShowRoundTransition(true);
                keySlots.SetActive(false);
                ShowRoundCleared();
                UpdateRoundNumber();
                UpdateTotalTaps(gm.TotalTaps);
                break;

            case GameState.InGame:
                PanelManager.Instance.ShowRoundTransition(false);
                keySlots.SetActive(true);
                break;

            case GameState.GameWon:
            case GameState.GameOver:
                keySlots.SetActive(false);
                break;
        }
    }

    public IEnumerator RunCountdown()
    {
        enterPressed = false;

        roundClearedText.gameObject.SetActive(true);
        complimentText.gameObject.SetActive(true);
        countdown.gameObject.SetActive(true);
        countdown.text = pressedEnterString.GetLocalizedString();
        yield return new WaitUntil(() => enterPressed);

        for (int i = 3; i > 0; i--)
        {
            SoundManager.Instance.PlaySFX("countdownSFX");
            countdownString.Arguments = new object[] { i };
            countdown.text = countdownString.GetLocalizedString();
            yield return new WaitForSeconds(1f);
        }

        SoundManager.Instance.PlaySFX("countdownOverSFX");
        countdown.text = startString.GetLocalizedString();
        yield return new WaitForSeconds(0.5f);

        complimentText.gameObject.SetActive(false);
    }

    public void ShowRoundCleared()
    {
        int round = gm.CurrentRound;

        if (round == 1)
        {
            roundClearedText.text = firstRoundString.GetLocalizedString();
            complimentText.text = warmUpString.GetLocalizedString();
        }
        else
        {
            int clearedRound = round - 1;
            roundClearedString.Arguments = new object[] { clearedRound };
            roundClearedText.text = roundClearedString.GetLocalizedString();

            int index = Random.Range(0, complimentStrings.Length);
            complimentText.text = complimentStrings[index].GetLocalizedString();
        }

        roundClearedText.gameObject.SetActive(true);
        complimentText.gameObject.SetActive(true);
    }

    private void UpdateRoundNumber()
    {
        roundString.Arguments = new object[] { gm.CurrentRound };
        roundString.RefreshString();
    }

    private void UpdateTotalTaps(int taps)
    {
        tapsString.Arguments = new object[] { taps };
        tapsString.RefreshString();
    }

    private void HandleGameEnded(bool won)
    {
        keySlots.SetActive(false);
        PanelManager.Instance.ShowEndScreen(won);
        PanelManager.Instance.ShowFinalTaps();
    }

    public void OpenUnityroomLink()
    {
        if (!string.IsNullOrEmpty(unityroomUrl))
            Application.OpenURL(unityroomUrl);
    }

    public void OpenGithubLink()
    {
        if (!string.IsNullOrEmpty(githubUrl))
            Application.OpenURL(githubUrl);
    }
}