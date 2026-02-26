using System;
using System.Collections;
using UnityEngine;
using unityroom.Api;

// ゲーム全体の進行状態を管理するステート
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
    // シングルトンインスタンス（ゲーム全体で1つのみ存在）
    public static GameManager Instance { get; private set; }

    // UIや他システムへ通知するイベント
    public event Action<int> OnTotalTapsChanged;
    public event Action OnRoundChanged;
    public event Action<bool> OnGameEnded;
    public event Action<bool> OnGamePaused;
    public event Action<GameState> OnGameStateChanged;

    [SerializeField] private InputReader input;
    [SerializeField] private RoundManager rounds;
    [SerializeField] private RoundUIController roundUIController;
    [SerializeField] private CameraShake cameraShake;

    public bool IsPaused => isPaused;

    [Header("Round details")]
    private int currentRoundIndex;
    private int totalRounds;
    private int totalTaps;

    // 現在のゲーム状態
    private GameState state = GameState.WaitingForStart;
    public GameState CurrentState => state;

    private bool isPaused = false;

    public int TotalTaps => totalTaps;
    public int CurrentRound => currentRoundIndex + 1;
    public int TotalRounds => totalRounds;

    // ラウンド開始前カウントダウン用コルーチン管理
    private Coroutine countdownCoroutine;

    private void Awake()
    {
        // シングルトン保証
        if (Instance != null)
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

    // ゲーム開始処理（初期化＋最初のラウンド準備）
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

    // 次のラウンドを取得して準備
    public void StartNextRound()
    {
        RoundDefinition nextRound = rounds.GetRound(currentRoundIndex);
        rounds.PrepareRound(nextRound, currentRoundIndex);
    }

    // リトライ時の完全リセット処理
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

    // 強制的にポーズ解除（状態を確実に戻す）
    private void ForceUnpause()
    {
        if (!isPaused)
            return;

        isPaused = false;
        Time.timeScale = 1f;
        SoundManager.Instance.ResumeAll();
        OnGamePaused?.Invoke(false);
    }

    // メインメニューへ戻る際の完全クリーンアップ処理
    public void ReturnToMainMenu()
    {
        if (SoundManager.Instance.musicSource.isPlaying)
        {
            SoundManager.Instance.musicSource.Stop();
            SoundManager.Instance.PlayMusic("mainMenuBGM");
        }

        // カウントダウンが動いていれば停止
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        // ラウンド強制終了
        if (rounds.IsRoundActive)
            rounds.EndRound(false);

        isPaused = false;
        Time.timeScale = 1f;
        SoundManager.Instance.ResumeAll();
        OnGamePaused?.Invoke(false);

        state = GameState.WaitingForStart;
        OnGameStateChanged?.Invoke(state);
    }

    // ポーズ切り替え（ゲーム中のみ有効）
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
        // ラウンド関連イベント購読
        rounds.OnRoundValidInput += HandleRoundValidInput;
        rounds.OnRoundInvalidInput += HandleRoundInvalidInput;
        rounds.OnRoundEnded += HandleRoundEnded;
        rounds.OnRoundPrepared += HandleRoundPrepared;

        input.OnKeyPressed += HandleKeyPressed;
    }

    private void OnDisable()
    {
        // イベント購読解除（メモリリーク防止）
        rounds.OnRoundValidInput -= HandleRoundValidInput;
        rounds.OnRoundInvalidInput -= HandleRoundInvalidInput;
        rounds.OnRoundEnded -= HandleRoundEnded;
        rounds.OnRoundPrepared -= HandleRoundPrepared;

        input.OnKeyPressed -= HandleKeyPressed;
    }

    // Escキーでポーズ制御
    private void HandleKeyPressed(KeyCode key)
    {
        if (key == KeyCode.Escape)
        {
            TogglePause();
            return;
        }
    }

    // ラウンド準備完了時の処理（カウントダウン開始）
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

    // ラウンド開始前のカウントダウン処理
    private IEnumerator RoundCountdownCoroutine()
    {
        // 安全のため時間スケールとポーズ状態を強制復帰
        isPaused = false;
        Time.timeScale = 1f;
        SoundManager.Instance.ResumeAll();

        yield return UIManager.Instance.RunCountdown();

        state = GameState.InGame;
        OnGameStateChanged?.Invoke(state);

        rounds.StartPreparedRound();
    }

    // 正解入力時の処理
    private void HandleRoundValidInput()
    {
        if (isPaused || state != GameState.InGame)
            return;

        SoundManager.Instance.PlaySFX("goodKeySFX");

        totalTaps++;
        totalTaps = Mathf.Min(totalTaps, 999); // 上限制限
        OnTotalTapsChanged?.Invoke(totalTaps);
    }

    // 不正解入力時の処理（演出付き）
    private void HandleRoundInvalidInput()
    {
        if (isPaused)
            return;

        SoundManager.Instance.PlaySFX("wrongKeySFX");
        cameraShake.Shake(0.1f, 0.1f);

        totalTaps++;
        totalTaps = Mathf.Min(totalTaps, 999);
        OnTotalTapsChanged?.Invoke(totalTaps);
    }

    // ラウンド終了時の分岐処理
    private void HandleRoundEnded(bool won)
    {
        if (won)
        {
            currentRoundIndex++;
            OnRoundChanged?.Invoke();

            // 全ラウンド終了ならゲーム終了
            if (currentRoundIndex >= rounds.TotalRounds)
            {
                EndGame(true);
            }
            else
            {
                StartNextRound();
            }
        }
        else
        {
            EndGame(false);
        }
    }

    // ゲーム終了処理（勝敗確定）
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

        // 勝利時のみスコア送信
        if (won)
            UnityroomApiClient.Instance.SendScore(1, totalTaps, ScoreboardWriteMode.HighScoreAsc);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}