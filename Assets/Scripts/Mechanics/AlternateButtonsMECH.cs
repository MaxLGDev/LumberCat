using System;
using UnityEngine;

public class AlternateButtonsMECH : IRoundMechanic
{
    public event Action OnValidInput;
    public event Action OnInvalidInput;
    public event Action OnCompleted;

    private int currentTaps;
    private int requiredTapsForRound;
    private bool isCompleted;
    private KeyCode lastKey;

    private KeyCode[] allowedKeys;

    public void StartRound(int requiredTaps, KeyCode[] allowedKeys)
    {
        this.requiredTapsForRound = requiredTaps;
        currentTaps = 0;
        isCompleted = false;
        lastKey = KeyCode.None;

        this.allowedKeys = allowedKeys;

        if (allowedKeys == null || allowedKeys.Length < 2)
            throw new ArgumentException("AlternateButtons requires 2 keys");
    }


    public void HandleKey(KeyCode key)
    {
        if (isCompleted)
            return;

        if (key != allowedKeys[0] && key != allowedKeys[1])
        {
            OnInvalidInput?.Invoke();
            return;
        }

        if (key == lastKey)
        {
            OnInvalidInput?.Invoke();
            return;
        }

        lastKey = key;
        currentTaps++;
        OnValidInput?.Invoke();

        if (currentTaps >= requiredTapsForRound)
        {
            isCompleted = true;
            OnCompleted?.Invoke();
        }
    }
}
