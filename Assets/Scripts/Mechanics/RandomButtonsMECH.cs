using System;
using UnityEngine;

public class RandomButtonsMECH : IRoundMechanic
{
    public event Action OnValidInput;
    public event Action OnInvalidInput;
    public event Action OnCompleted;
    public event Action<KeyCode[]> OnCurrentKeyChanged;

    private int currentTaps;
    private int requiredTapsForRound;
    private bool isCompleted;

    private KeyCode[] keyPool;
    private KeyCode currentKey;
    private KeyCode nextKey;

    public KeyCode CurrentKey => currentKey;

    public void StartRound(int requiredTaps, KeyCode[] allowedKeys)
    {
        this.requiredTapsForRound = requiredTaps;
        currentTaps = 0;
        isCompleted = false;

        if (allowedKeys == null || allowedKeys.Length < 2)
            throw new ArgumentException("The key pool requires more than 2 keys");

        keyPool = allowedKeys;

        currentKey = GetRandomKeyDifferentFrom(KeyCode.None);
        nextKey = GetRandomKeyDifferentFrom(currentKey);

        OnCurrentKeyChanged?.Invoke(new KeyCode[] { currentKey, nextKey });
    }

    public void HandleKey(KeyCode key)
    {
        if (isCompleted)
            return;

        if(key != currentKey)
        {
            OnInvalidInput?.Invoke();
            return;
        }

        currentTaps++;
        OnValidInput?.Invoke();

        if (currentTaps >= requiredTapsForRound)
        {
            isCompleted = true;
            OnCompleted?.Invoke();
            return;
        }

        currentKey = nextKey;
        nextKey = GetRandomKeyDifferentFrom(currentKey);

        OnCurrentKeyChanged?.Invoke(new[] { currentKey, nextKey });
    }

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
