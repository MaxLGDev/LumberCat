using System;
using UnityEngine;

public class SplitPhaseMECH : IRoundMechanic
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

    // Expose the currently active key for UI
    public KeyCode CurrentKey => currentKey;

    public void StartRound(int requiredTaps, KeyCode[] allowedKeys)
    {
        requiredTapsForRound = requiredTaps;

        if (allowedKeys == null || allowedKeys.Length < 2)
            throw new ArgumentException("SplitPhase requires at least 2 keys");

        // Pick 2 random keys
        int indexA = UnityEngine.Random.Range(0, allowedKeys.Length);
        int indexB;
        do
        {
            indexB = UnityEngine.Random.Range(0, allowedKeys.Length);
        } while (indexB == indexA);

        keyA = allowedKeys[indexA];
        keyB = allowedKeys[indexB];

        currentTaps = 0;
        isCompleted = false;

        currentKey = keyA; // first half starts with keyA
        // Send both keys to UI so it can display them
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

        // Switch to second key at halfway point
        if (currentTaps == (requiredTapsForRound + 1) / 2)
        {
            currentKey = keyB;
            // Keep both keys visible, UI highlights the active one
            OnCurrentKeyChanged?.Invoke(new KeyCode[] { keyA, keyB });
        }
    }
}
