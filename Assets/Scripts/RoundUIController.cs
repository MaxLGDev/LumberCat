using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using System;

public class RoundPresentation
{
    public string instructionText;
    public string description;
}

public class RoundUIController : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;

    [Header("UI Details")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text timerDuration;

    [SerializeField] private LocalizedString timerInactiveString;
    [SerializeField] private LocalizedString timerActiveString;

    [SerializeField] private SpriteRenderer[] keySlots;
    [SerializeField] private SpriteRenderer[] keyOutlines;
    [SerializeField] private Sprite[] keySprites;

    private string timerActiveFormat;
    private string timerInactiveText;

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
    }


    private void Update()
    {

        if (!roundManager.IsRoundActive)
            return;

        if (string.IsNullOrEmpty(timerActiveFormat))
            return;

        timerDuration.text = $"{timerActiveFormat}{roundManager.RemainingTime:F3}";
    }

    private void ShowAllowedKeys(KeyCode[] keys)
    {
        if (keySlots == null || keySlots.Length == 0) return;

        int slotCount = keySlots.Length;

        // Clear all slots
        for (int i = 0; i < slotCount; i++)
        {
            keySlots[i].sprite = null;
            keySlots[i].color = Color.white;

            if (keyOutlines != null && i < keyOutlines.Length)
                keyOutlines[i].color = Color.clear;
        }

        if (keys == null || keys.Length == 0) return;

        // Get active key from mechanic
        KeyCode activeKey = KeyCode.None;
        if (roundManager.CurrentMechanic != null)
            activeKey = roundManager.CurrentMechanic.CurrentKey;

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
                keySlots[slot].sprite = keySprite;

            // Highlight outline
            if (keyOutlines != null && slot < keyOutlines.Length)
            {
                keyOutlines[slot].color = (keys[i] == activeKey) ? Color.green : Color.red;
            }
        }
    }

    // Map KeyCode to your sprites safely
    private Sprite GetSpriteForKey(KeyCode key)
    {
        foreach (var sprite in keySprites)
        {
            if (sprite.name.Equals(key.ToString(), StringComparison.OrdinalIgnoreCase))
                return sprite;
        }
        return null; // fallback
    }




    public void IncrementRoundBar()
    {
        progressBar.value += 1;
    }

    private void OnEnable()
    {
        roundManager.OnRoundStarted += InitializeRoundUI;
        roundManager.OnRoundValidInput += IncrementRoundBar;
        roundManager.OnRoundEnded += ShowRoundEnd;
        roundManager.OnActiveKeysChanged += ShowAllowedKeys;
    }

    private void ShowRoundEnd(bool won)
    {
        timerDuration.text = timerInactiveText;

        if (won)
            Debug.Log("gg");
        else
            Debug.Log("unlucky");
    }

    private void InitializeRoundUI(RoundDefinition round)
    {
        progressBar.minValue = 0;
        progressBar.maxValue = round.requiredTaps;
        progressBar.value = 0;

        Debug.Log($"[DEBUG] Required taps for this round: {round.requiredTaps}");

        if (roundManager.CurrentMechanic != null)
            roundManager.CurrentMechanic.OnCurrentKeyChanged += ShowAllowedKeys;
    }

    public void Reset()
    {
        progressBar.value = 0;
    }

    private int KeyCodeToIndex(KeyCode key)
    {
        if (key >= KeyCode.A && key <= KeyCode.Z)
            return key - KeyCode.A;  // 0..25
        if (key == KeyCode.Space)
            return 26;
        return -1; // unsupported
    }
}
