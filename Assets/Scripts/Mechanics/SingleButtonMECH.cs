using System;
using UnityEngine;

public class SingleButtonMECH : IRoundMechanic
{
    public event Action OnValidInput;
    public event Action OnInvalidInput;
    public event Action<KeyCode[]> OnCurrentKeyChanged;

    private KeyCode[] keyPool;
    private KeyCode allowedKey;

    public KeyCode CurrentKey => allowedKey;

    public void StartRound(KeyCode[] allowedKeys)
    {
        keyPool = allowedKeys;
        GetRandomKey();
        OnCurrentKeyChanged?.Invoke(new KeyCode[] { allowedKey });
    }

    public void HandleKey(KeyCode key)
    {
        if (key != allowedKey)
        {
            OnInvalidInput?.Invoke();
            return;
        }

        OnValidInput?.Invoke();
    }

    private void GetRandomKey()
    {
        int poolIndex = UnityEngine.Random.Range(0, keyPool.Length);
        allowedKey = keyPool[poolIndex];
    }
}
