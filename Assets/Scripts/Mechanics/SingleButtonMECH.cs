using System;
using UnityEngine;

public class SingleButtonMECH : IRoundMechanic
{
    public event Action OnValidInput;
    public event Action OnInvalidInput;
    public event Action OnCompleted;

    private int currentTaps;
    private int requiredTapsForRound;
    private bool isCompleted;
    private KeyCode allowedKey;

    public void StartRound(int requiredTaps, KeyCode[] allowedKeys)
    {
        this.requiredTapsForRound = requiredTaps;
        currentTaps = 0;
        isCompleted = false;

        allowedKey = allowedKeys[0];
    }

    public void HandleKey(KeyCode key)
    {
        if (isCompleted)
            return;

        if(key == allowedKey)
        {
            currentTaps++;
            OnValidInput?.Invoke();

            if (currentTaps >= requiredTapsForRound && isCompleted == false)
            {
                isCompleted = true;
                OnCompleted?.Invoke();
            }
        }
        else
        {
            OnInvalidInput?.Invoke();
        }
    }
}
