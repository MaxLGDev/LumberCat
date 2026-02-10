using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance { get; private set; }

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TutorialUI tutorialUI;

    [Header("In-Game")]
    [SerializeField] private GameObject inGamePanel;
    [SerializeField] private GameObject roundTransitionPanel;

    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;

    [Header("Final Screen")]
    [SerializeField] private GameObject finalScreenPanel;
    [SerializeField] private GameObject victoryText;
    [SerializeField] private GameObject defeatText;
    [SerializeField] private TMP_Text finalTapsText;

    [Header("Options")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private OptionsUI optionsUI;

    [Header("Localization")]
    [SerializeField] private LocalizedString tapsString;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple Panel Managers detected!");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGamePaused += HandleGamePaused;
        else
            Debug.LogError("GameManager.Instance is null in PanelManager.Start()");
    }

    public void ShowFinalTaps()
    {
        int totalTaps = GameManager.Instance.TotalTaps;

        tapsString.Arguments = new object[] { totalTaps };
        finalTapsText.text = tapsString.GetLocalizedString();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGamePaused -= HandleGamePaused;
    }

    private void HandleGamePaused(bool isPaused)
    {
        ShowPause(isPaused);
    }

    #region "Toggle Panels"

    public void ShowMainMenu()
    {
        if (mainMenuPanel == null)
            throw new MissingReferenceException("Main Menu Panel is missing!");

        GameManager.Instance.ReturnToMainMenu();

        SoundManager.Instance.PlayMusic("mainMenuBGM");

        HideAll();
        mainMenuPanel.SetActive(true);
    }

    public void ShowInGame()
    {
        if (inGamePanel == null)
            throw new MissingReferenceException("In-Game Panel is missing!");

        HideAll();
        inGamePanel.SetActive(true);
    }

    public void ShowTutorial(bool show)
    {
        if (tutorialPanel == null)
            throw new MissingReferenceException("Rules Panel Panel is missing!");

        tutorialPanel.SetActive(true);

        if (show)
            tutorialUI.ShowFade();
        else
            tutorialUI.HideFade();
    }

    public void ShowPause(bool show)
    {
        if (pausePanel == null)
            throw new MissingReferenceException("Pause Panel is missing!");

        pausePanel.SetActive(show);
    }

    public void ShowRoundTransition(bool show)
    {
        if (roundTransitionPanel == null)
            throw new MissingReferenceException("Round Transition Panel is missing!");

        roundTransitionPanel.SetActive(show);
    }

    public void ShowEndScreen(bool won)
    {
        if (finalScreenPanel == null)
            throw new MissingReferenceException("Final Screen Panel is missing!");

        HideAll();
        finalScreenPanel.SetActive(true);

        if (won)
            victoryText.SetActive(true);
        else
            defeatText.SetActive(true);

    }

    public void ShowOptions(bool show)
    {
        if (optionsPanel == null)
            throw new MissingReferenceException("Options Panel is missing!");

        optionsPanel.SetActive(true);

        if(show)
            optionsUI.ShowFade();
        else
            optionsUI.HideFade();
    }

    public void HideAll()
    {
        mainMenuPanel.SetActive(false);
        tutorialPanel.SetActive(false);
        roundTransitionPanel.SetActive(false);
        pausePanel.SetActive(false);
        victoryText.SetActive(false);
        defeatText.SetActive(false);
        inGamePanel.SetActive(false);
        finalScreenPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    #endregion
}