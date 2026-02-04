using System;
using UnityEngine;

public interface IRoundMechanic
{
    event System.Action OnValidInput;
    event System.Action OnInvalidInput;
    event System.Action OnCompleted;

    void StartRound(int requiredTaps, KeyCode[] allowedKeys);
    void HandleKey(KeyCode key);

    event Action<KeyCode[]> OnCurrentKeyChanged;
    KeyCode CurrentKey { get; }
}
