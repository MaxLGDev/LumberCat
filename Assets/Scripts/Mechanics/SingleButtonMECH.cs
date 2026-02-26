using System;
using UnityEngine;

// 1つのキーのみを正解とする最もシンプルなメカニック
public class SingleButtonMECH : IRoundMechanic
{
    public event Action OnValidInput;
    public event Action OnInvalidInput;

    // 現在の有効キーが設定されたときにUIへ通知
    public event Action<KeyCode[]> OnCurrentKeyChanged;

    private KeyCode[] keyPool;   // 使用可能なキー一覧
    private KeyCode allowedKey;  // 現在の正解キー

    public KeyCode CurrentKey => allowedKey;

    public void StartRound(KeyCode[] allowedKeys)
    {
        keyPool = allowedKeys;

        // ランダムに正解キーを決定
        GetRandomKey();

        // UIへ現在のキーを通知（単一キーなので配列サイズ1）
        OnCurrentKeyChanged?.Invoke(new KeyCode[] { allowedKey });
    }

    public void HandleKey(KeyCode key)
    {
        // 正解キー以外は不正解
        if (key != allowedKey)
        {
            OnInvalidInput?.Invoke();
            return;
        }

        OnValidInput?.Invoke();
    }

    // keyPoolからランダムに1つ選択
    private void GetRandomKey()
    {
        int poolIndex = UnityEngine.Random.Range(0, keyPool.Length);
        allowedKey = keyPool[poolIndex];
    }
}