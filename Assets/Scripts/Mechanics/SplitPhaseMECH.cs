using System;
using UnityEngine;

// 進行度によってキーが切り替わる2段階メカニック
// 前半はkeyA、50%到達でkeyBへ移行する
public class SplitPhaseMECH : IRoundMechanic, IProgressAware
{
    public event Action OnValidInput;
    public event Action OnInvalidInput;
    public event Action<KeyCode[]> OnCurrentKeyChanged;

    private KeyCode keyA;
    private KeyCode keyB;
    private KeyCode currentKey;

    private bool inSecondPhase;

    // 現在有効なキーを外部へ公開
    public KeyCode CurrentKey => currentKey;

    public void StartRound(KeyCode[] allowedKeys)
    {
        // 2段階構成のため最低2キー必要
        if (allowedKeys == null || allowedKeys.Length < 2)
            throw new ArgumentException("SplitPhase requires at least 2 keys");

        inSecondPhase = false;

        // 重複しない2キーをランダム選択
        int indexA = UnityEngine.Random.Range(0, allowedKeys.Length);
        int indexB;
        do
        {
            indexB = UnityEngine.Random.Range(0, allowedKeys.Length);
        } while (indexB == indexA);

        keyA = allowedKeys[indexA];
        keyB = allowedKeys[indexB];

        // 前半はkeyAから開始
        currentKey = keyA;

        // UIへ両キーを通知（強調はCurrentKey参照）
        OnCurrentKeyChanged?.Invoke(new KeyCode[] { keyA, keyB });
    }

    public void HandleKey(KeyCode key)
    {
        if (key != currentKey)
        {
            // フェーズ1ではミス時にkeyAへリセット
            if (!inSecondPhase)
            {
                currentKey = keyA;
                OnCurrentKeyChanged?.Invoke(new[] { keyA, keyB });
            }

            // フェーズ2ではリセットせずkeyBを維持
            OnInvalidInput?.Invoke();
            return;
        }

        OnValidInput?.Invoke();
    }

    // ラウンド進行度に応じてフェーズを切り替える
    public void OnProgressChanged(int current, int required)
    {
        int halfPoint = required / 2;

        // 50%以上でフェーズ2へ移行
        if (!inSecondPhase && current >= halfPoint)
        {
            inSecondPhase = true;
            currentKey = keyB;
            OnCurrentKeyChanged?.Invoke(new[] { keyA, keyB });
        }
        // ヒステリシスを持たせ、少し下回った程度では戻らない（約45%未満で戻す）
        else if (inSecondPhase && current < halfPoint - (required / 10))
        {
            inSecondPhase = false;
            currentKey = keyA;
            OnCurrentKeyChanged?.Invoke(new[] { keyA, keyB });
        }
    }
}