using System;
using UnityEngine;

// 2つのキーを交互に押させるメカニック
public class AlternateButtonsMECH : IRoundMechanic
{
    public event Action OnValidInput;
    public event Action OnInvalidInput;

    // 表示中の2キーが更新されたときにUIへ通知
    public event Action<KeyCode[]> OnCurrentKeyChanged;

    private KeyCode keyA;
    private KeyCode keyB;
    private KeyCode currentKey;

    // 現在押すべきキー（緑表示）
    public KeyCode CurrentKey => currentKey;

    // このラウンドで使用される2キー
    public KeyCode[] AllowedKeys => new KeyCode[] { keyA, keyB };

    public void StartRound(KeyCode[] allowedKeys)
    {
        // 交互動作のため最低2キー必要
        if (allowedKeys == null || allowedKeys.Length < 2)
            throw new ArgumentException("AlternateButtons requires at least 2 keys");

        // 重複しない2キーをランダム選択
        int first = UnityEngine.Random.Range(0, allowedKeys.Length);
        int second;

        do
        {
            second = UnityEngine.Random.Range(0, allowedKeys.Length);
        }
        while (second == first);

        keyA = allowedKeys[first];
        keyB = allowedKeys[second];

        // 初期の正解キーはランダムで決定
        currentKey = UnityEngine.Random.value < 0.5f ? keyA : keyB;

        // UIへ2キーを通知（CurrentKeyを強調表示）
        OnCurrentKeyChanged?.Invoke(new KeyCode[] { keyA, keyB });
    }

    public void HandleKey(KeyCode key)
    {
        // 現在キーと一致しなければ不正解
        if (key != currentKey)
        {
            OnInvalidInput?.Invoke();
            return;
        }

        OnValidInput?.Invoke();

        // 正解後、もう一方のキーへ切り替え（交互動作）
        currentKey = (currentKey == keyA) ? keyB : keyA;

        OnCurrentKeyChanged?.Invoke(new KeyCode[] { keyA, keyB });
    }
}