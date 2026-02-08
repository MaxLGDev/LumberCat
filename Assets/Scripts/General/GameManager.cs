using System;
using UnityEngine;

enum GameState
{
    WaitingForStart,
    Cooldown,
    InGame,
    GameWon,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<int> OnTotalTapsChanged;
    public event Action OnRoundChanged;
    public event Action<bool> OnRoundStarted;
    public event Action<bool> OnGameEnded;
    public event Action<bool> OnGamePaused;

    [SerializeField] private InputReader input;
    [SerializeField] private RoundManager rounds;
    [SerializeField] private RoundUIController roundUIController;

    [Header("Round details")]
    private int currentRoundIndex;
    private int totalRounds;
    private int totalTaps;
    private GameState state = GameState.WaitingForStart;

    private bool isPaused = false;

    public int TotalTaps => totalTaps;
    public int CurrentRound => currentRoundIndex + 1;
    public int TotalRounds => totalRounds;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("Multiple Game Managers detected!");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        state = GameState.WaitingForStart;
    }

    private void Start()
    {
        SoundManager.Instance.PlayMusic("mainMenuBGM");
    }

    //private int invalidTaps
    public void StartGame()
    {
        rounds.ShuffleRounds();

        totalTaps = 0;
        currentRoundIndex = 0;
        totalRounds = rounds.TotalRounds;
        state = GameState.InGame;

        OnRoundStarted?.Invoke(true);
        PanelManager.Instance.ShowInGame();

        StartNextRound();
    }

    public void StartNextRound()
    {
        RoundDefinition nextRound = rounds.GetRound(currentRoundIndex);
        rounds.PrepareRound(nextRound, currentRoundIndex);
    }

    public void RetryGame()
    {
        if (rounds.IsRoundActive)
            rounds.ResetCurrentRound();

        rounds.ShuffleRounds();

        totalTaps = 0;
        currentRoundIndex = 0;
        totalRounds = rounds.TotalRounds;
        state = GameState.InGame;

        OnTotalTapsChanged?.Invoke(TotalTaps);
        OnRoundStarted?.Invoke(true);
        roundUIController.ResetTimerUI();

        PanelManager.Instance.ShowRoundTransition(true);

        RoundDefinition nextRound = rounds.GetRound(0);
        rounds.PrepareRound(nextRound, 0);
    }

    public void TogglePause()
    {
        if (state != GameState.InGame)
            return;

        isPaused = !isPaused;



        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
            SoundManager.Instance.PauseAll();
        else
            SoundManager.Instance.ResumeAll();

        OnGamePaused?.Invoke(isPaused);
    }

    private void OnEnable()
    {
        rounds.OnRoundValidInput += HandleRoundValidInput;
        rounds.OnRoundInvalidInput += HandleRoundInvalidInput;
        rounds.OnRoundEnded += HandleRoundEnded;

        input.OnKeyPressed += HandleKeyPressed;
    }

    private void OnDisable()
    {
        rounds.OnRoundValidInput -= HandleRoundValidInput;
        rounds.OnRoundInvalidInput -= HandleRoundInvalidInput;
        rounds.OnRoundEnded -= HandleRoundEnded;

        input.OnKeyPressed -= HandleKeyPressed;
    }

    private void HandleKeyPressed(KeyCode key)
    {
        if (key == KeyCode.Escape)
            TogglePause();
    }

    private void HandleRoundValidInput()
    {
        if (isPaused)
            return;

        totalTaps++;
        totalTaps = Mathf.Min(totalTaps, 999);
        OnTotalTapsChanged?.Invoke(totalTaps);
    }

    private void HandleRoundInvalidInput()
    {
        if (isPaused)
            return;

        totalTaps++;
        totalTaps = Mathf.Min(totalTaps, 999);
        OnTotalTapsChanged?.Invoke(totalTaps);
    }

    private void HandleRoundEnded(bool won)
    {
        if(won)
        {
            currentRoundIndex++;
            OnRoundChanged?.Invoke();

            if (currentRoundIndex >= rounds.TotalRounds)
                EndGame(true);
            else
                StartNextRound();
        }
        else
        {
            EndGame(false);
        }
    }

    private void EndGame(bool won)
    {
        state = won ? GameState.GameWon : GameState.GameOver;

        if (isPaused)
            TogglePause();

        if (rounds.IsRoundActive)
            rounds.EndRound(false);

        OnGameEnded?.Invoke(won);
        Debug.Log(won ? "You won!" : "You lost!");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
