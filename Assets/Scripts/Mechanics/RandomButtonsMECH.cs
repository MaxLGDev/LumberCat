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
    private KeyCode allowedKey;
    private KeyCode lastKey;

    public KeyCode CurrentKey => allowedKey;

    public void StartRound(int requiredTaps, KeyCode[] allowedKeys)
    {
        this.requiredTapsForRound = requiredTaps;
        currentTaps = 0;
        isCompleted = false;

        if (allowedKeys == null || allowedKeys.Length < 2)
            throw new ArgumentException("The key pool requires more than 2 keys");

        keyPool = allowedKeys;

        GetRandomKey();

        OnCurrentKeyChanged?.Invoke(new KeyCode[] { allowedKey });
    }

    public void HandleKey(KeyCode key)
    {
        if (isCompleted)
            return;

        if(key != allowedKey)
        {
            OnInvalidInput?.Invoke();
            return;
        }

        currentTaps++;
        OnValidInput?.Invoke();
        lastKey = allowedKey;

        GetRandomKey();

        while (allowedKey == lastKey) GetRandomKey();

        // Fire event every time key changes
        OnCurrentKeyChanged?.Invoke(new KeyCode[] { allowedKey });

        if (currentTaps >= requiredTapsForRound)
        {
            isCompleted = true;
            OnCompleted?.Invoke();
        }
    }

    private void GetRandomKey()
    {
        int poolIndex = UnityEngine.Random.Range(0, keyPool.Length);

        allowedKey = keyPool[poolIndex];
    }
}
