using System;
using UnityEngine;

public class CorrectKeyMECH : IRoundMechanic, ITickable
{
    public event Action OnValidInput;
    public event Action OnInvalidInput;
    public event Action OnCompleted;
    public event Action<KeyCode[]> OnCurrentKeyChanged;

    private int currentTaps;
    private int requiredTapsForRound;
    private bool isCompleted;

    private KeyCode[] keyPool;
    private KeyCode[] chosenKeys;
    private KeyCode allowedKey;       // the currently green key
    private KeyCode lastGreenKey;

    private float switchTimer;
    private float nextSwitchTime;

    // Expose the currently active (green) key
    public KeyCode CurrentKey => allowedKey;

    public void StartRound(int requiredTaps, KeyCode[] allowedKeys)
    {
        requiredTapsForRound = requiredTaps;

        if (allowedKeys == null || allowedKeys.Length < 3)
            throw new ArgumentException("The key pool requires at least 3 keys");

        this.requiredTapsForRound = requiredTaps;
        currentTaps = 0;
        isCompleted = false;

        keyPool = allowedKeys;
        chosenKeys = new KeyCode[3];

        RollChosenKeys();
        PickGreenKey();

        switchTimer = 0f;
        ScheduleNextSwitch();

        // Notify UI of the initial keys
        OnCurrentKeyChanged?.Invoke(chosenKeys);

        Debug.Log($"Keys: {chosenKeys[0]}, {chosenKeys[1]}, {chosenKeys[2]} | Green: {allowedKey}");
    }

    public void HandleKey(KeyCode key)
    {
        if (isCompleted) return;

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

    public void Tick(float deltaTime)
    {
        if (isCompleted) return;

        switchTimer += deltaTime;

        if (switchTimer >= nextSwitchTime)
        {
            switchTimer = 0f;
            ScheduleNextSwitch();
            PickGreenKey();
            // UI will automatically highlight CurrentKey
        }
    }

    private void ScheduleNextSwitch()
    {
        nextSwitchTime = UnityEngine.Random.Range(1f, 2.5f); // random switch interval
    }

    private void RollChosenKeys()
    {
        for (int i = 0; i < chosenKeys.Length; i++)
        {
            KeyCode candidate;
            bool duplicate;
            do
            {
                candidate = keyPool[UnityEngine.Random.Range(0, keyPool.Length)];
                duplicate = false;
                for (int j = 0; j < i; j++)
                {
                    if (chosenKeys[j] == candidate)
                    {
                        duplicate = true;
                        break;
                    }
                }
            } while (duplicate);

            chosenKeys[i] = candidate;
        }
    }

    private void PickGreenKey()
    {
        KeyCode next;
        do
        {
            next = chosenKeys[UnityEngine.Random.Range(0, chosenKeys.Length)];
        } while (next == lastGreenKey);

        allowedKey = next;
        lastGreenKey = next;

        // Always send all keys so UI can display them, highlighting CurrentKey
        OnCurrentKeyChanged?.Invoke(chosenKeys);

        Debug.Log($"Keys: {chosenKeys[0]}, {chosenKeys[1]}, {chosenKeys[2]} | Green: {allowedKey}");
    }
}
