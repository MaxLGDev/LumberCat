using System;
using UnityEngine;

// 3つのキーの中からランダムに「正解キー（緑）」を選び、一定時間ごとに切り替えるメカニック
public class CorrectKeyMECH : IRoundMechanic, ITickable
{
    public event Action OnValidInput;
    public event Action OnInvalidInput;

    // 現在表示中のキー配列が変わったときにUIへ通知
    public event Action<KeyCode[]> OnCurrentKeyChanged;

    private KeyCode[] keyPool;     // 使用可能な全キー候補
    private KeyCode[] chosenKeys;  // 実際にこのラウンドで使用する3キー

    private KeyCode allowedKey;    // 現在の正解キー（緑）
    private KeyCode lastGreenKey;  // 直前の正解キー（連続同一回避用）

    private float switchTimer;
    private float nextSwitchTime;

    // 現在の正解キーを外部へ公開
    public KeyCode CurrentKey => allowedKey;

    public void StartRound(KeyCode[] allowedKeys)
    {
        // 最低3キー必要（重複なしで3つ選ぶため）
        if (allowedKeys == null || allowedKeys.Length < 3)
            throw new ArgumentException("The key pool requires at least 3 keys");

        keyPool = allowedKeys;
        chosenKeys = new KeyCode[3];

        RollChosenKeys();  // 重複なしで3キー抽選
        PickGreenKey();    // 初期の正解キー決定

        switchTimer = 0f;
        ScheduleNextSwitch();

        // 初期表示用にUIへ通知
        OnCurrentKeyChanged?.Invoke(chosenKeys);

        Debug.Log($"Keys: {chosenKeys[0]}, {chosenKeys[1]}, {chosenKeys[2]} | Green: {allowedKey}");
    }

    public void HandleKey(KeyCode key)
    {
        // 正解キー以外なら不正解
        if (key != allowedKey)
        {
            OnInvalidInput?.Invoke();
            return;
        }

        OnValidInput?.Invoke();
    }

    // 一定時間経過ごとに正解キーを切り替える
    public void Tick(float deltaTime)
    {
        switchTimer += deltaTime;

        if (switchTimer >= nextSwitchTime)
        {
            switchTimer = 0f;
            ScheduleNextSwitch();
            PickGreenKey();
            // UIはCurrentKeyを参照してハイライト更新
        }
    }

    private void ScheduleNextSwitch()
    {
        // 次の切り替えまでの時間をランダム設定
        nextSwitchTime = UnityEngine.Random.Range(1f, 2.5f);
    }

    // keyPoolから重複なしで3キーを抽選
    private void RollChosenKeys()
    {
        for (int i = 0; i < chosenKeys.Length; i++)
        {
            KeyCode candidate;
            bool duplicate;

            do
            {
                candidate = keyPool[UnityEngine.Random.Range(0, keyPool.Length)];
                duplicate = false;

                // すでに選ばれていないかチェック
                for (int j = 0; j < i; j++)
                {
                    if (chosenKeys[j] == candidate)
                    {
                        duplicate = true;
                        break;
                    }
                }

            } while (duplicate);

            chosenKeys[i] = candidate;
        }
    }

    // 直前と異なるキーを正解キーとして選択
    private void PickGreenKey()
    {
        KeyCode next;

        do
        {
            next = chosenKeys[UnityEngine.Random.Range(0, chosenKeys.Length)];
        }
        while (next == lastGreenKey);

        allowedKey = next;
        lastGreenKey = next;

        // 常に全キーを通知（UI側でCurrentKeyを強調表示）
        OnCurrentKeyChanged?.Invoke(chosenKeys);
    }
}