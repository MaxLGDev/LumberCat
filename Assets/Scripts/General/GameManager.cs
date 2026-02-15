using System;
using System.Collections;
using UnityEngine;
using unityroom.Api;

public enum GameState
{
    WaitingForStart,
    RoundTransition,
    InGame,
    GameWon,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<int> OnTotalTapsChanged;
    public event Action OnRoundChanged;
    public event Action<bool> OnGameEnded;
    public event Action<bool> OnGamePaused;
    public event Action<GameState> OnGameStateChanged;

    [SerializeField] private InputReader input;
    [SerializeField] private RoundManager rounds;
    [SerializeField] private RoundUIController roundUIController;

    public bool IsPaused => isPaused;

    [Header("Round details")]
    private int currentRoundIndex;
    private int totalRounds;
    private int totalTaps;
    private GameState state = GameState.WaitingForStart;
    public GameState CurrentState => state;

    private bool isPaused = false;

    public int TotalTaps => totalTaps;
    public int CurrentRound => currentRoundIndex + 1;
    public int TotalRounds => totalRounds;

    private Coroutine countdownCoroutine;

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

        OnRoundChanged?.Invoke();

        state = GameState.RoundTransition;
        OnGameStateChanged?.Invoke(state);

        PanelManager.Instance.ShowInGame();

        StartNextRound();
        SoundManager.Instance.PlayMusic("inGameBGM");
    }

    public void StartNextRound()
    {
        RoundDefinition nextRound = rounds.GetRound(currentRoundIndex);
        rounds.PrepareRound(nextRound, currentRoundIndex);
    }

    public void RetryGame()
    {
        ForceUnpause();

        if (SoundManager.Instance.musicSource.isPlaying)
        {
            SoundManager.Instance.musicSource.Stop();
            SoundManager.Instance.PlayMusic("inGameBGM");
        }

        if (rounds.IsRoundActive)
            rounds.ResetCurrentRound();

        rounds.ShuffleRounds();

        totalTaps = 0;
        currentRoundIndex = 0;
        totalRounds = rounds.TotalRounds;
        state = GameState.RoundTransition;
        OnGameStateChanged?.Invoke(state);

        OnTotalTapsChanged?.Invoke(TotalTaps);
        roundUIController.ResetTimerUI();

        RoundDefinition nextRound = rounds.GetRound(0);
        rounds.PrepareRound(nextRound, 0);
    }

    private void ForceUnpause()
    {
        if (!isPaused)
            return;

        isPaused = false;
        Time.timeScale = 1f;
        SoundManager.Instance.ResumeAll();
        OnGamePaused?.Invoke(false);
    }

    public void ReturnToMainMenu()
    {
        if (SoundManager.Instance.musicSource.isPlaying)
        {
            SoundManager.Instance.musicSource.Stop();
            SoundManager.Instance.PlayMusic("mainMenuBGM");
        }

        // Stop countdown if running
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        // FORCE stop any round
        if (rounds.IsRoundActive)
            rounds.EndRound(false);

        isPaused = false;
        Time.timeScale = 1f;
        SoundManager.Instance.ResumeAll();
        OnGamePaused?.Invoke(false);

        state = GameState.WaitingForStart;
        OnGameStateChanged?.Invoke(state);
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
        rounds.OnRoundPrepared += HandleRoundPrepared;

        input.OnKeyPressed += HandleKeyPressed;
    }

    private void OnDisable()
    {
        rounds.OnRoundValidInput -= HandleRoundValidInput;
        rounds.OnRoundInvalidInput -= HandleRoundInvalidInput;
        rounds.OnRoundEnded -= HandleRoundEnded;
        rounds.OnRoundPrepared -= HandleRoundPrepared;

        input.OnKeyPressed -= HandleKeyPressed;
    }

    private void HandleKeyPressed(KeyCode key)
    {
        if (key == KeyCode.Escape)
        {
            TogglePause();
            return;
        }
    }

    private void HandleRoundPrepared()
    {
        state = GameState.RoundTransition;
        OnGameStateChanged?.Invoke(state);

        PanelManager.Instance.ShowRoundTransition(true);

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        countdownCoroutine = StartCoroutine(RoundCountdownCoroutine());
    }

    private IEnumerator RoundCountdownCoroutine()
    {
        // Safety: force unpause & resume time
        isPaused = false;
        Time.timeScale = 1f;
        SoundManager.Instance.ResumeAll();

        yield return UIManager.Instance.RunCountdown();

        state = GameState.InGame;
        OnGameStateChanged?.Invoke(state);
        rounds.StartPreparedRound();
    }

    private void HandleRoundValidInput()
    {
        if (isPaused)
            return;

        if (state != GameState.InGame)
            return;

        SoundManager.Instance.PlaySFX("goodKeySFX");

        totalTaps++;
        totalTaps = Mathf.Min(totalTaps, 999);
        OnTotalTapsChanged?.Invoke(totalTaps);
    }

    private void HandleRoundInvalidInput()
    {
        if (isPaused)
            return;

        SoundManager.Instance.PlaySFX("wrongKeySFX");

        totalTaps++;
        totalTaps = Mathf.Min(totalTaps, 999);
        OnTotalTapsChanged?.Invoke(totalTaps);
    }

    private void HandleRoundEnded(bool won)
    {
        if (won)
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
        if (SoundManager.Instance.musicSource.isPlaying)
        {
            SoundManager.Instance.musicSource.Stop();
            SoundManager.Instance.PlayMusic("finalScreenBGM");
        }

        state = won ? GameState.GameWon : GameState.GameOver;
        OnGameStateChanged?.Invoke(state);

        if (isPaused)
            TogglePause();

        if (rounds.IsRoundActive)
            rounds.EndRound(false);

        OnGameEnded?.Invoke(won);
        Debug.Log(won ? "You won!" : "You lost!");

        if (won)
            UnityroomApiClient.Instance.SendScore(1, totalTaps, ScoreboardWriteMode.HighScoreAsc);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
