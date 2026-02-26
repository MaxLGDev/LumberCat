using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

// ゲーム中UI全体の制御・表示更新を行うクラス
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
    private bool localizationReady;

    private void Awake()
    {
        // シングルトン管理
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gm = GameManager.Instance;

        // ローカライズ変更時に自動でUIへ反映
        roundString.StringChanged += value => currentRound.text = value;
        tapsString.StringChanged += value => totalTaps.text = value;

        pressedEnterString.StringChanged += value => countdown.text = value;
        countdownString.StringChanged += value => countdown.text = value;
        startString.StringChanged += value => countdown.text = value;

        firstRoundString.StringChanged += value => roundClearedText.text = value;
        warmUpString.StringChanged += value => complimentText.text = value;

        roundClearedString.StringChanged += value => roundClearedText.text = value;
    }

    private IEnumerator Start()
    {
        // ローカライズ初期化待機
        yield return LocalizationSettings.InitializationOperation;

        localizationReady = true;

        keySlots.SetActive(false);
        PanelManager.Instance.ShowMainMenu();
    }

    private void Update()
    {
        if (!localizationReady)
            return;

        // ラウンド遷移中にEnter入力検出
        if (Input.GetKeyDown(KeyCode.Return) &&
            gm.CurrentState == GameState.RoundTransition)
        {
            enterPressed = true;
        }
    }

    private void OnEnable()
    {
        if (gm == null)
            return;

        // GameManagerイベント購読
        gm.OnTotalTapsChanged += UpdateTotalTaps;
        gm.OnGameEnded += HandleGameEnded;
        gm.OnGameStateChanged += HandleGameStateChanged;
        gm.OnRoundChanged += UpdateRoundNumber;
    }

    private void OnDisable()
    {
        if (gm == null)
            return;

        // イベント解除
        gm.OnTotalTapsChanged -= UpdateTotalTaps;
        gm.OnGameEnded -= HandleGameEnded;
        gm.OnGameStateChanged -= HandleGameStateChanged;
        gm.OnRoundChanged -= UpdateRoundNumber;
    }

    // ゲーム状態変化時のUI制御
    private void HandleGameStateChanged(GameState state)
    {
        if (!localizationReady)
            return;

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

    // カウントダウン演出
    public IEnumerator RunCountdown()
    {
        if (!localizationReady)
            yield break;

        enterPressed = false;

        roundClearedText.gameObject.SetActive(true);
        complimentText.gameObject.SetActive(true);
        countdown.gameObject.SetActive(true);

        // 「Enterを押して開始」
        pressedEnterString.RefreshString();
        yield return new WaitUntil(() => enterPressed);

        // 3→2→1 カウントダウン
        for (int i = 3; i > 0; i--)
        {
            SoundManager.Instance.PlaySFX("countdownSFX");

            countdownString.Arguments = new object[] { i };
            countdownString.RefreshString();

            yield return new WaitForSeconds(1f);
        }

        // スタート表示
        SoundManager.Instance.PlaySFX("countdownOverSFX");
        startString.RefreshString();
        yield return new WaitForSeconds(0.5f);

        complimentText.gameObject.SetActive(false);
    }

    // ラウンドクリア表示
    public void ShowRoundCleared()
    {
        if (!localizationReady)
            return;

        int round = gm.CurrentRound;

        if (round == 1)
        {
            firstRoundString.RefreshString();
            warmUpString.RefreshString();
        }
        else
        {
            int clearedRound = round - 1;

            roundClearedString.Arguments = new object[] { clearedRound };
            roundClearedString.RefreshString();

            // ランダム褒めメッセージ
            int index = Random.Range(0, complimentStrings.Length);
            complimentStrings[index].StringChanged += value => complimentText.text = value;
            complimentStrings[index].RefreshString();
        }

        if (gm.CurrentRound != 1)
            SoundManager.Instance.PlaySFX("roundVictorySFX");

        roundClearedText.gameObject.SetActive(true);
        complimentText.gameObject.SetActive(true);
    }

    // ラウンド番号更新
    private void UpdateRoundNumber()
    {
        if (!localizationReady)
            return;

        roundString.Arguments = new object[] { gm.CurrentRound };
        roundString.RefreshString();
    }

    // タップ数更新
    private void UpdateTotalTaps(int taps)
    {
        if (!localizationReady)
            return;

        tapsString.Arguments = new object[] { taps };
        tapsString.RefreshString();
    }

    // ゲーム終了時処理
    private void HandleGameEnded(bool won)
    {
        keySlots.SetActive(false);
        PanelManager.Instance.ShowEndScreen(won);
        PanelManager.Instance.ShowFinalTaps();
    }

    // 外部リンク（unityroom）
    public void OpenUnityroomLink()
    {
        if (!string.IsNullOrEmpty(unityroomUrl))
            Application.OpenURL(unityroomUrl);
    }

    // 外部リンク（GitHub）
    public void OpenGithubLink()
    {
        if (!string.IsNullOrEmpty(githubUrl))
            Application.OpenURL(githubUrl);
    }
}