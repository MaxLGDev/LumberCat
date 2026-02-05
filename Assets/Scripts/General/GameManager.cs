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
    //public event Action<bool> OnGamePaused;

    [SerializeField] private UIManager UIManager;
    [SerializeField] private InputReader input;
    [SerializeField] private RoundManager rounds;

    [Header("Round details")]
    private int currentRoundIndex;
    private int totalRounds;
    private int totalTaps;
    private GameState state = GameState.WaitingForStart;

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

    //private int invalidTaps
    public void StartGame()
    {
        rounds.ShuffleRounds();

        totalTaps = 0;
        currentRoundIndex = 0;
        totalRounds = rounds.TotalRounds;
        state = GameState.InGame;

        OnRoundStarted?.Invoke(true);
        StartNextRound();
    }

    public void StartNextRound()
    {
        RoundDefinition nextRound = rounds.GetRound(currentRoundIndex);
        rounds.PrepareRound(nextRound, currentRoundIndex);
    }

    private void OnEnable()
    {
        rounds.OnRoundValidInput += HandleRoundValidInput;
        rounds.OnRoundInvalidInput += HandleRoundInvalidInput;
        rounds.OnRoundEnded += HandleRoundEnded;
    }

    private void OnDisable()
    {
        rounds.OnRoundValidInput -= HandleRoundValidInput;
        rounds.OnRoundInvalidInput -= HandleRoundInvalidInput;
        rounds.OnRoundEnded -= HandleRoundEnded;
    }

    private void HandleRoundValidInput()
    {
        totalTaps++;
        totalTaps = Mathf.Max(totalTaps, 999);
        OnTotalTapsChanged?.Invoke(totalTaps);
    }
    private void HandleRoundInvalidInput()
    {
        totalTaps++;
        totalTaps = Mathf.Max(totalTaps, 999);
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

        if (rounds.IsRoundActive)
            rounds.EndRound(false);

        OnGameEnded?.Invoke(won);
        Debug.Log(won ? "You won!" : "You lost!");
    }
}
