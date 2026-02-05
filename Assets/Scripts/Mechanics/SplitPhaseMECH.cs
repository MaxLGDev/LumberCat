using System;
using UnityEngine;

public class SplitPhaseMECH : IRoundMechanic, IProgressAware
{
    public event Action OnValidInput;
    public event Action OnInvalidInput;
    public event Action<KeyCode[]> OnCurrentKeyChanged;

    private KeyCode keyA;
    private KeyCode keyB;
    private KeyCode currentKey;

    private bool inSecondPhase;

    // Expose the currently active key for UI
    public KeyCode CurrentKey => currentKey;

    public void StartRound(KeyCode[] allowedKeys)
    {
        if (allowedKeys == null || allowedKeys.Length < 2)
            throw new ArgumentException("SplitPhase requires at least 2 keys");

        inSecondPhase = false;

        // Pick 2 random keys
        int indexA = UnityEngine.Random.Range(0, allowedKeys.Length);
        int indexB;
        do
        {
            indexB = UnityEngine.Random.Range(0, allowedKeys.Length);
        } while (indexB == indexA);

        keyA = allowedKeys[indexA];
        keyB = allowedKeys[indexB];

        currentKey = keyA; // first half starts with keyA
        // Send both keys to UI so it can display them
        OnCurrentKeyChanged?.Invoke(new KeyCode[] { keyA, keyB });
    }

    public void HandleKey(KeyCode key)
    {
        if (key != currentKey)
        {
            // Phase 1: reset (allowed)
            if (!inSecondPhase)
            {
                currentKey = keyA;
                OnCurrentKeyChanged?.Invoke(new[] { keyA, keyB });
            }

            // Phase 2: NO RESET, key stays keyB
            OnInvalidInput?.Invoke();
            return;
        }

        OnValidInput?.Invoke();
    }

    public void OnProgressChanged(int current, int required)
    {
        int halfPoint = required / 2;

        // Enter phase 2 at 50%
        if (!inSecondPhase && current >= halfPoint)
        {
            inSecondPhase = true;
            currentKey = keyB;
            OnCurrentKeyChanged?.Invoke(new[] { keyA, keyB });
        }
        // Only drop back to phase 1 if you fall significantly below (45%)
        else if (inSecondPhase && current < halfPoint - (required / 10))
        {
            inSecondPhase = false;
            currentKey = keyA;
            OnCurrentKeyChanged?.Invoke(new[] { keyA, keyB });
        }
    }
}
