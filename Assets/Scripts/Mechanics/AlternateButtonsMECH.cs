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
        requiredTapsForRound = requiredTaps;
        if (allowedKeys == null || allowedKeys.Length < 2)
            throw new ArgumentException("AlternateButtons requires at least 2 keys");

        keyA = allowedKeys[0];
        keyB = allowedKeys[1];

        currentTaps = 0;
        isCompleted = false;

        currentKey = keyA; // always start with first key
        OnCurrentKeyChanged?.Invoke(new KeyCode[] { currentKey });
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
        OnCurrentKeyChanged?.Invoke(new KeyCode[] { currentKey });
    }
}
