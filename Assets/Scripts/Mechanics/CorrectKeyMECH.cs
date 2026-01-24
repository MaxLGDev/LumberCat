using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CorrectKeyMECH : IRoundMechanic, ITickable
{
    public event Action OnValidInput;
    public event Action OnInvalidInput;
    public event Action OnCompleted;

    private int currentTaps;
    private int requiredTapsForRound;
    private bool isCompleted;

    private KeyCode[] keyPool;
    private KeyCode[] chosenKeys;
    private KeyCode allowedKey;
    private KeyCode lastGreenKey;

    private float switchTimer;
    private float nextSwitchTime;

    public void StartRound(int requiredTaps, KeyCode[] allowedKeys)
    {
        this.requiredTapsForRound = requiredTaps;
        currentTaps = 0;
        isCompleted = false;

        if (allowedKeys == null || allowedKeys.Length < 3)
            throw new ArgumentException("The key pool requires at least than 3 keys");

        keyPool = allowedKeys;

        chosenKeys = new KeyCode[3];
        RollChosenKeys();
        PickGreenKey();

        switchTimer = 0f;
        ScheduleNextSwitch();

        Debug.Log($"Keys: {chosenKeys[0]}, {chosenKeys[1]}, {chosenKeys[2]} | Green: {allowedKey}");
    }

    public void HandleKey(KeyCode key)
    {
        Debug.Log(key);
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

    public void Tick(float deltaTime)
    {
        if (isCompleted)
            return;

        switchTimer += deltaTime;

        if(switchTimer >= nextSwitchTime)
        {
            switchTimer = 0f;
            ScheduleNextSwitch();
            PickGreenKey();
        }
    }

    private void ScheduleNextSwitch()
    {
        nextSwitchTime = UnityEngine.Random.Range(1f, 1.5f);
    }

    private void RollChosenKeys()
    {
        for(int i = 0; i < chosenKeys.Length; i++)
        {
            KeyCode candidate;
            bool duplicate;

            do
            {
                candidate = keyPool[UnityEngine.Random.Range(0, keyPool.Length)];
                duplicate = false;

                for(int j = 0; j < i; j++)
                {
                    if (chosenKeys[j] == candidate)
                    {
                        duplicate = true;
                        break;
                    }
                }
            }  while (duplicate);

            chosenKeys[i] = candidate;
        }
    }

    private void PickGreenKey()
    {
        KeyCode next;

        do
        {
            next = chosenKeys[UnityEngine.Random.Range(0, chosenKeys.Length)];
        }
        while (next == lastGreenKey);

        allowedKey = next;
        lastGreenKey = next;

        Debug.Log($"Keys: {chosenKeys[0]}, {chosenKeys[1]}, {chosenKeys[2]} | Green: {allowedKey}");
    }



    // Pick 3 keys -> Enable 1, disable 2
    // After a random amount of time (not less than 300ms), reroll the green key among the 3 keys
    // A new one becomes green, the other 2 become or stay red
    // Eg: A E T -> A is green, E and T are red -> Spam A while it's green
    // After 750ms -> Keys are rerolled, A becomes red, E stays red, T becomes green
    // Spam until complete

}
