using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance { get; private set; }

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject rulesPanel;

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

    public void ShowRules()
    {
        if (rulesPanel == null)
            throw new MissingReferenceException("Rules Panel Panel is missing!");

        rulesPanel.SetActive(true);
    }

    public void ShowPause(bool show)
    {
        if (pausePanel == null)
            throw new MissingReferenceException("Pause Panel is missing!");

        Debug.Log("ShowPause called with: " + show);

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
        if (won)
            victoryText.SetActive(true);
        else
            defeatText.SetActive(true);

        finalScreenPanel.SetActive(true);
    }

    public void ShowOptions()
    {
        if (optionsPanel == null)
            throw new MissingReferenceException("Options Panel is missing!");

        optionsPanel.SetActive(true);
    }

    public void HideAll()
    {
        mainMenuPanel.SetActive(false);
        rulesPanel.SetActive(false);
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