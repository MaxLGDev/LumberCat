using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class RoundPresentation
{
    public string instructionText;
    public string description;
}

[Serializable]
public struct KeySpritePair
{
    public KeyCode key;
    public Sprite sprite;
}

public class RoundUIController : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;

    [SerializeField] private KeySpritePair[] keySpritePairs;

    private Dictionary<KeyCode, Sprite> keySpriteMap;

    [Header("UI Details")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text timerDuration;
    [SerializeField] private TMP_Text mechanicName;

    [SerializeField] private LocalizedString timerInactiveString;
    [SerializeField] private LocalizedString timerActiveString;

    [SerializeField] private SpriteRenderer[] keySlots;
    [SerializeField] private SpriteRenderer[] keyOutlines;
    [SerializeField] private Sprite[] keySprites;

    private string timerActiveFormat;
    private string timerInactiveText;

    private IRoundMechanic lastMechanic;

    private KeyCode[] lastKeys;

    private void Awake()
    {
        timerInactiveString.StringChanged += value =>
        {
            timerInactiveText = value;
            timerDuration.text = value;
        };

        timerActiveString.StringChanged += value =>
        {
            timerActiveFormat = value;
        };

        timerInactiveString.RefreshString();
        timerActiveString.RefreshString();

        // -- Build dictionary map --
        keySpriteMap = new Dictionary<KeyCode, Sprite>();

        foreach (var pair in keySpritePairs)
        {
            if (pair.sprite == null)
            {
                Debug.LogError($"KeySpritePair for {pair.key} has NO sprite assigned", this);
                continue;
            }

            if (keySpriteMap.ContainsKey(pair.key))
            {
                Debug.LogError($"Duplicate sprite mapping for key {pair.key}", this);
                continue;
            }

            keySpriteMap.Add(pair.key, pair.sprite);
        }
    }


    private void Update()
    {

        if (!roundManager.IsRoundActive)
            return;

        if (string.IsNullOrEmpty(timerActiveFormat))
            return;

        progressBar.value = roundManager.CurrentProgress;

        timerDuration.text = $"{timerActiveFormat}{roundManager.RemainingTime:F3}";
    }

    private void ShowAllowedKeys(KeyCode[] keys)
    {
        if (keySlots == null || keySlots.Length == 0) return;

        lastKeys = keys;

        int slotCount = keySlots.Length;

        if (keys == null || keys.Length == 0) return;

        // Clear all slots
        for (int i = 0; i < slotCount; i++)
        {
            keySlots[i].enabled = false;


            if (keyOutlines != null && i < keyOutlines.Length)
                keyOutlines[i].color = Color.clear;
        }

        // Get active key from mechanic
        KeyCode activeKey = KeyCode.None;
        if (roundManager.CurrentMechanic != null)
            activeKey = roundManager.CurrentMechanic.CurrentKey;

        string mechanicType = roundManager.CurrentMechanic != null ? roundManager.CurrentMechanic.GetType().Name : "NULL";

        // Determine which slots to use (simple centering for 1–3 keys)
        int[] slotsToUse;
        if (keys.Length == 1) slotsToUse = new int[] { 1 };
        else if (keys.Length == 2) slotsToUse = new int[] { 0, 2 };
        else slotsToUse = new int[] { 0, 1, 2 };

        for (int i = 0; i < keys.Length && i < slotsToUse.Length; i++)
        {
            int slot = slotsToUse[i];
            if (slot >= slotCount) continue;

            // Get the correct sprite for the key
            Sprite keySprite = GetSpriteForKey(keys[i]);
            if (keySprite != null)
            {
                keySlots[slot].sprite = keySprite;
                keySlots[slot].enabled = true;
            }

            // Highlight outline
            if (keyOutlines != null && slot < keyOutlines.Length)
            {
                Color correctColor = new Color(0.3f, 0.8f, 0.3f); // Softer green
                Color wrongColor = new Color(0.8f, 0.3f, 0.3f);   // Softer red
                keyOutlines[slot].color = (keys[i] == activeKey) ? correctColor : wrongColor;
            }
        }
    }

    // Map KeyCode to your sprites safely
    private Sprite GetSpriteForKey(KeyCode key)
    {
        if (keySpriteMap == null)
        {
            Debug.LogError("Key sprite map not initialized", this);
            return null;
        }

        if (!keySpriteMap.TryGetValue(key, out var sprite))
        {
            Debug.LogError($"No sprite mapped for key: {key}");
            return null;
        }

        return sprite;
    }

    private void OnEnable()
    {
        roundManager.OnRoundStarted += InitializeRoundUI;
        roundManager.OnRoundEnded += ShowRoundEnd;
        roundManager.OnActiveKeysChanged += ShowAllowedKeys;
    }

    private void OnDisable()
    {
        roundManager.OnRoundStarted -= InitializeRoundUI;
        roundManager.OnRoundEnded -= ShowRoundEnd;
        roundManager.OnActiveKeysChanged -= ShowAllowedKeys;

        if (roundManager.CurrentMechanic != null)
            roundManager.CurrentMechanic.OnCurrentKeyChanged -= ShowAllowedKeys;
    }

    private void ShowRoundEnd(bool won)
    {
        timerDuration.text = timerInactiveText;

        ShowAllowedKeys(Array.Empty<KeyCode>());

        if (won)
            Debug.Log("gg");
        else
            Debug.Log("unlucky");
    }

    private void InitializeRoundUI()
    {
        lastKeys = null;

        // Unsubscribe from the previous mechanic
        if (lastMechanic != null)
        {
            lastMechanic.OnCurrentKeyChanged -= ShowAllowedKeys;
        }

        mechanicName.text = roundManager.CurrentMechanicType.ToString();
        progressBar.minValue = 0;
        progressBar.maxValue = roundManager.CurrentRequiredTaps;
        progressBar.value = 0;

        // Subscribe to the new mechanic
        if (roundManager.CurrentMechanic != null)
        {
            lastMechanic = roundManager.CurrentMechanic;
            roundManager.CurrentMechanic.OnCurrentKeyChanged += ShowAllowedKeys;
        }
    }

    public void Reset()
    {
        progressBar.value = 0;
    }
}
