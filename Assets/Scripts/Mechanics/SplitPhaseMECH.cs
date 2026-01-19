using System;
using UnityEngine;

public class SplitPhaseMECH : IRoundMechanic
{
    public event Action OnValidInput;
    public event Action OnInvalidInput;
    public event Action OnCompleted;

    private int currentTaps;
    private int requiredTapsForRound;
    private bool isCompleted;

    private KeyCode keyA;
    private KeyCode keyB;

    public void StartRound(int requiredTaps, KeyCode[] allowedKeys)
    {
        this.requiredTapsForRound = requiredTaps;
        currentTaps = 0;
        isCompleted = false;

        if (allowedKeys == null || allowedKeys.Length < 2)
            throw new ArgumentException("AlternateButtons requires 2 keys");

        keyA = allowedKeys[0];
        keyB = allowedKeys[1];
    }

    public void HandleKey(KeyCode key)
    {
        if (isCompleted)
            return;

        if (key != keyA && key != keyB)
        {
            OnInvalidInput?.Invoke();
            return;
        }

        if (!InSecondPhase && key != keyA)
        {
            OnInvalidInput?.Invoke();
            return;
        }
        else if(InSecondPhase && key != keyB)
        {
            OnInvalidInput?.Invoke();
            return;
        }

        currentTaps++;
        OnValidInput?.Invoke();

        if (currentTaps >= requiredTapsForRound && isCompleted == false)
        {
            isCompleted = true;
            OnCompleted?.Invoke();
        }
    }

    int SplitPoint => (requiredTapsForRound + 1) / 2;
    bool InSecondPhase => currentTaps >= SplitPoint;
}
