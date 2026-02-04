using System;
using UnityEngine;

public class SingleButtonMECH : IRoundMechanic
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

    public KeyCode CurrentKey => allowedKey;

    public void StartRound(int requiredTaps, KeyCode[] allowedKeys)
    {
        this.requiredTapsForRound = requiredTaps;
        currentTaps = 0;
        isCompleted = false;

        keyPool = allowedKeys;
        GetRandomKey();

        OnCurrentKeyChanged?.Invoke(new KeyCode[] { allowedKey });
    }

    public void HandleKey(KeyCode key)
    {
        if (isCompleted)
            return;

        if (key != allowedKey)
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
        }
    }

    private void GetRandomKey()
    {
        int poolIndex = UnityEngine.Random.Range(0, keyPool.Length);

        allowedKey = keyPool[poolIndex];
    }
}
