using System;
using UnityEngine;

public class AlternateButtonsMECH : IRoundMechanic
{
    public event Action OnValidInput;
    public event Action OnInvalidInput;
    public event Action OnCompleted;
    public event Action<KeyCode[]> OnCurrentKeyChanged;

    private int currentTaps;
    private int requiredTapsForRound;
    private bool isCompleted;

    private KeyCode keyA;
    private KeyCode keyB;
    private KeyCode currentKey;

    public KeyCode CurrentKey => currentKey;          // the green key
    public KeyCode[] AllowedKeys => new KeyCode[] { keyA, keyB };

    public void StartRound(int requiredTaps, KeyCode[] allowedKeys)
    {
        if (allowedKeys == null || allowedKeys.Length < 2)
            throw new ArgumentException("AlternateButtons requires at least 2 keys");

        requiredTapsForRound = requiredTaps;
        currentTaps = 0;
        isCompleted = false;

        int first = UnityEngine.Random.Range(0, allowedKeys.Length);
        int second;

        do
        {
            second = UnityEngine.Random.Range(0, allowedKeys.Length);
        }
        while (second == first);

        keyA = allowedKeys[first];
        keyB = allowedKeys[second];

        currentKey = UnityEngine.Random.value < 0.5f ? keyA : keyB;

        OnCurrentKeyChanged?.Invoke(new KeyCode[] { keyA, keyB });
    }

    public void HandleKey(KeyCode key)
    {
        if (isCompleted) return;

        if (key != currentKey)
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

        // Alternate keys
        currentKey = (currentKey == keyA) ? keyB : keyA;
        OnCurrentKeyChanged?.Invoke(new KeyCode[] { keyA, keyB });
    }
}
