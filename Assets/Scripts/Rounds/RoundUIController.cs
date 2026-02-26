using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

// ラウンド表示用のデータ構造
public class RoundPresentation
{
    public string instructionText;
    public string description;
}

[Serializable]
public struct KeySpritePair
{
    public KeyCode key;
    public Sprite sprite;
}

public class RoundUIController : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;

    // KeyCodeと対応Spriteのマッピング定義（Inspector設定）
    [SerializeField] private KeySpritePair[] keySpritePairs;

    private Dictionary<KeyCode, Sprite> keySpriteMap;

    [Header("UI Details")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text timerDuration;
    [SerializeField] private TMP_Text mechanicName;
    [SerializeField] private LocalizedString mechanicNameLocalized;

    [SerializeField] private LocalizedString timerInactiveString;
    [SerializeField] private LocalizedString timerActiveString;

    [SerializeField] private SpriteRenderer[] keySlots;
    [SerializeField] private SpriteRenderer[] keyOutlines;

    private string timerActiveFormat;
    private string timerInactiveText;

    private IRoundMechanic lastMechanic;
    private KeyCode[] lastKeys;

    private void Awake()
    {
        // ローカライズ文字列更新時の反映処理
        timerInactiveString.StringChanged += value =>
        {
            timerInactiveText = value;
            timerDuration.text = value;
        };

        timerActiveString.StringChanged += value =>
        {
            timerActiveFormat = value;
        };

        mechanicNameLocalized.StringChanged += value =>
        {
            mechanicName.text = value;
        };

        timerInactiveString.RefreshString();
        timerActiveString.RefreshString();

        if (roundManager == null)
        {
            Debug.LogError("RoundUIController: RoundManager not assigned", this);
            enabled = false;
            return;
        }

        // KeyCode → Sprite の辞書を構築
        keySpriteMap = new Dictionary<KeyCode, Sprite>();

        foreach (var pair in keySpritePairs)
        {
            if (pair.sprite == null)
            {
                Debug.LogError($"KeySpritePair for {pair.key} has NO sprite assigned", this);
                continue;
            }

            if (keySpriteMap.ContainsKey(pair.key))
            {
                Debug.LogError($"Duplicate sprite mapping for key {pair.key}", this);
                continue;
            }

            keySpriteMap.Add(pair.key, pair.sprite);
        }
    }

    private void Update()
    {
        // ラウンド中のみUI更新
        if (!roundManager.IsRoundActive)
            return;

        if (string.IsNullOrEmpty(timerActiveFormat))
            return;

        progressBar.value = roundManager.CurrentProgress;

        // 残り時間表示（フォーマットはローカライズ文字列）
        timerDuration.text = $"{timerActiveFormat}{roundManager.RemainingTime:F3}";
    }

    // 現在有効なキーをUIに表示
    private void ShowAllowedKeys(KeyCode[] keys)
    {
        if (keySlots == null || keySlots.Length == 0)
            return;

        if (keys == null || keys.Length == 0)
            return;

        lastKeys = keys;

        int slotCount = keySlots.Length;

        // 全スロットを初期化
        for (int i = 0; i < slotCount; i++)
        {
            keySlots[i].enabled = false;

            if (keyOutlines != null && i < keyOutlines.Length)
                keyOutlines[i].color = Color.clear;
        }

        // 現在のアクティブキー取得（強調表示用）
        KeyCode activeKey = KeyCode.None;
        if (roundManager.CurrentMechanic != null)
            activeKey = roundManager.CurrentMechanic.CurrentKey;

        // 1〜3キーの簡易中央配置ロジック
        int[] slotsToUse;
        if (keys.Length == 1) slotsToUse = new int[] { 1 };
        else if (keys.Length == 2) slotsToUse = new int[] { 0, 2 };
        else slotsToUse = new int[] { 0, 1, 2 };

        for (int i = 0; i < keys.Length && i < slotsToUse.Length; i++)
        {
            int slot = slotsToUse[i];
            if (slot >= slotCount) continue;

            Sprite keySprite = GetSpriteForKey(keys[i]);
            if (keySprite != null)
            {
                keySlots[slot].sprite = keySprite;
                keySlots[slot].enabled = true;
            }

            // 正解キーを緑、それ以外を赤でアウトライン表示
            if (keyOutlines != null && slot < keyOutlines.Length)
            {
                Color correctColor = new Color(0.3f, 0.8f, 0.3f);
                Color wrongColor = new Color(0.8f, 0.3f, 0.3f);
                keyOutlines[slot].color = (keys[i] == activeKey) ? correctColor : wrongColor;
            }
        }
    }

    public void SetCountdownRaw(string text)
    {
        timerDuration.text = text;
    }

    // KeyCodeに対応するSpriteを安全に取得
    private Sprite GetSpriteForKey(KeyCode key)
    {
        if (keySpriteMap == null)
        {
            Debug.LogError("Key sprite map not initialized", this);
            return null;
        }

        if (!keySpriteMap.TryGetValue(key, out var sprite))
        {
            Debug.LogError($"No sprite mapped for key: {key}");
            return null;
        }

        return sprite;
    }

    private void OnEnable()
    {
        // ラウンドイベント購読
        roundManager.OnRoundPrepared += InitializeRoundUI;
        roundManager.OnRoundEnded += ShowRoundEnd;
        roundManager.OnActiveKeysChanged += ShowAllowedKeys;
    }

    private void OnDisable()
    {
        // イベント購読解除
        roundManager.OnRoundPrepared -= InitializeRoundUI;
        roundManager.OnRoundEnded -= ShowRoundEnd;
        roundManager.OnActiveKeysChanged -= ShowAllowedKeys;

        if (roundManager.CurrentMechanic != null)
            roundManager.CurrentMechanic.OnCurrentKeyChanged -= ShowAllowedKeys;
    }

    private void ShowRoundEnd(bool won)
    {
        timerDuration.text = timerInactiveText;

        // キー表示クリア
        ShowAllowedKeys(Array.Empty<KeyCode>());

        if (won)
            Debug.Log("gg");
        else
            Debug.Log("unlucky");
    }

    public void ResetTimerUI()
    {
        timerDuration.text = timerInactiveText;
    }

    // 新しいラウンド開始時のUI初期化
    private void InitializeRoundUI()
    {
        lastKeys = null;

        // 以前のメカニックからイベント解除
        if (lastMechanic != null)
        {
            lastMechanic.OnCurrentKeyChanged -= ShowAllowedKeys;
        }

        LocalizeMechanicName();

        progressBar.minValue = 0;
        progressBar.maxValue = roundManager.CurrentRequiredTaps;
        progressBar.value = 0;

        // 新しいメカニックへイベント登録
        if (roundManager.CurrentMechanic != null)
        {
            lastMechanic = roundManager.CurrentMechanic;
            roundManager.CurrentMechanic.OnCurrentKeyChanged += ShowAllowedKeys;
        }
    }

    // メカニック名をローカライズテーブルから取得
    private void LocalizeMechanicName()
    {
        mechanicNameLocalized.TableReference = "MechanicsTable";
        mechanicNameLocalized.TableEntryReference = "mech." + roundManager.CurrentMechanicType.ToString();
        mechanicNameLocalized.RefreshString();
    }

    public void Reset()
    {
        progressBar.value = 0;
    }
}