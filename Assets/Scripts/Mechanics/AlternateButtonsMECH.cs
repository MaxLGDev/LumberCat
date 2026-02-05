using System;
using UnityEngine;

public class AlternateButtonsMECH : IRoundMechanic
{
    public event Action OnValidInput;
    public event Action OnInvalidInput;
    public event Action<KeyCode[]> OnCurrentKeyChanged;

    private KeyCode keyA;
    private KeyCode keyB;
    private KeyCode currentKey;

    public KeyCode CurrentKey => currentKey;          // the green key
    public KeyCode[] AllowedKeys => new KeyCode[] { keyA, keyB };

    public void StartRound(KeyCode[] allowedKeys)
    {
        if (allowedKeys == null || allowedKeys.Length < 2)
            throw new ArgumentException("AlternateButtons requires at least 2 keys");

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
        if (key != currentKey)
        {
            OnInvalidInput?.Invoke();
            return;
        }

        OnValidInput?.Invoke();

        // Alternate keys
        currentKey = (currentKey == keyA) ? keyB : keyA;
        OnCurrentKeyChanged?.Invoke(new KeyCode[] { keyA, keyB });
    }
}
