using System;
using UnityEngine;

// 現在のキーと次のキーを表示し、正解入力ごとに切り替えるメカニック
public class RandomButtonsMECH : IRoundMechanic
{
    public event Action OnValidInput;
    public event Action OnInvalidInput;

    // 現在表示中のキー（current / next）が変わったときに通知
    public event Action<KeyCode[]> OnCurrentKeyChanged;

    private KeyCode[] keyPool;  // 使用可能なキー一覧
    private KeyCode currentKey;
    private KeyCode nextKey;

    // 現在押すべきキー
    public KeyCode CurrentKey => currentKey;

    public void StartRound(KeyCode[] allowedKeys)
    {
        // 少なくとも2キー必要（currentとnextを分けるため）
        if (allowedKeys == null || allowedKeys.Length < 2)
            throw new ArgumentException("The key pool requires more than 2 keys");

        keyPool = allowedKeys;

        // 初期キー設定（Noneと重複しないようにする）
        currentKey = GetRandomKeyDifferentFrom(KeyCode.None);
        nextKey = GetRandomKeyDifferentFrom(currentKey);

        // UIへ現在と次のキーを通知
        OnCurrentKeyChanged?.Invoke(new KeyCode[] { currentKey, nextKey });
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

        // 正解後、nextをcurrentに昇格させ、新しいnextを生成
        currentKey = nextKey;
        nextKey = GetRandomKeyDifferentFrom(currentKey);

        OnCurrentKeyChanged?.Invoke(new[] { currentKey, nextKey });
    }

    // 指定キーと異なるランダムキーを取得
    private KeyCode GetRandomKeyDifferentFrom(KeyCode avoid)
    {
        KeyCode key;
        do
        {
            key = keyPool[UnityEngine.Random.Range(0, keyPool.Length)];
        }
        while (key == avoid);

        return key;
    }
}