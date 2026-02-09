using System;
using UnityEngine;

public enum RoundMechanic
{
    SingleKey,
    AlternateKeys,
    SplitKeys,
    KeySequence,
    CorrectKey
}

[System.Serializable]
public class RoundDefinition
{
    public RoundMechanic mechanic;

    [Tooltip("Time limit in seconds")]
    public float duration;

    [Tooltip("Taps required to clear the round")]
    public int requiredTaps;

    public KeyCode[] allowedKeys;

    [Header("Difficulty Modifiers")]
    [Tooltip("Multiplier for tap count (1.0 = normal, 1.5 = 50% more taps)")]
    public float tapMultiplier = 1f;

    [Tooltip("Multiplier for time (1.0 = normal, 2.0 = double time)")]
    public float timeMultiplier = 1f;
}

public class RoundManager : MonoBehaviour
{
    public event Action OnRoundValidInput;
    public event Action OnRoundInvalidInput;
    public event Action<bool> OnRoundEnded;
    public event Action<KeyCode[]> OnActiveKeysChanged;
    public event Action OnRoundPrepared;
    private Action<KeyCode[]> currentKeyChangedHandler;

    private IRoundMechanic currentMechanic;
    private ITickable tickable;

    [SerializeField] private InputController inputController;
    [SerializeField] private RoundDefinition[] rounds;
    private RoundDefinition[] shuffledRounds;
    private RoundDefinition currentRound;

    [Header("Difficulty Scale")]
    [SerializeField] private int baseTaps = 30;
    [SerializeField] private int maxTaps = 60;
    [SerializeField] private float baseTime = 15f;
    [SerializeField] private float minTime = 6f;
    public int CurrentProgress { get; private set; }

    public IRoundMechanic CurrentMechanic => currentMechanic;
    public RoundMechanic CurrentMechanicType { get; private set; }

    private float timer;
    private bool isActive;

    public int CurrentRequiredTaps {get; private set;}
    public int CurrentTaps { get; private set;}

    public bool IsRoundActive => isActive;
    public float RemainingTime => timer;
    public int TotalRounds => rounds.Length;

    private void Update()
    {
        if (!isActive)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
            LoseRound();

        tickable?.Tick(Time.deltaTime);
    }

    public void ShuffleRounds()
    {
        shuffledRounds = (RoundDefinition[])rounds.Clone();
        int n = shuffledRounds.Length;

        for(int i = 0; i < n - 1; i++)
        {
            int j = UnityEngine.Random.Range(i, n);
            var temp = shuffledRounds[i];
            shuffledRounds[i] = shuffledRounds[j];
            shuffledRounds[j] = temp;
        }
    }

    public void PrepareRound(RoundDefinition round, int roundIndex)
    {
        float t = (float)(roundIndex + 1) / (shuffledRounds.Length - 1);

        int baseTapsPerRound = Mathf.RoundToInt(Mathf.Lerp(baseTaps, maxTaps, t));
        int taps = Mathf.RoundToInt(baseTapsPerRound * round.tapMultiplier);

        float calculedTime = Mathf.Lerp(baseTime, minTime, t);
        float time = baseTime * round.timeMultiplier;

        if (round.mechanic == RoundMechanic.KeySequence)
            time *= 3f;

        if (round.mechanic == RoundMechanic.SingleKey)
            time /= 2f;

        if (round.mechanic == RoundMechanic.AlternateKeys)
            time /= 2f;

        if (round.mechanic == RoundMechanic.SplitKeys)
            time /= 2f;

        CurrentProgress = 0;
        CurrentRequiredTaps = baseTapsPerRound;

        currentRound = round;
        currentRound.duration = time;

        CurrentMechanicType = round.mechanic;

        // Clear old mechanic first if it exists
        if (currentMechanic != null)
        {
            UnhookMechanic(currentMechanic);
            currentMechanic = null;
        }

        OnActiveKeysChanged?.Invoke(Array.Empty<KeyCode>());

        currentMechanic = CreateMechanic(round.mechanic);
        tickable = currentMechanic as ITickable;
        HookMechanic(currentMechanic);

        inputController.Bind(currentMechanic.HandleKey);
        inputController.EnableInput(false);

        currentMechanic.StartRound(round.allowedKeys);

        OnRoundPrepared?.Invoke();
    }

    public void StartPreparedRound ()
    {
        if (currentMechanic == null)
            return;

        timer = currentRound.duration;
        inputController.EnableInput(true);
        isActive = true;
    }

    public void EndRound(bool won)
    {
        isActive = false;
        tickable = null;

        inputController.EnableInput(false);
        inputController.Unbind();

        if(currentMechanic != null)
        {
            UnhookMechanic(currentMechanic);
            currentMechanic = null;
        }

        OnRoundEnded?.Invoke(won);
    }

    public RoundDefinition GetRound(int index)
    {
        if (shuffledRounds == null || index < 0 || index >= shuffledRounds.Length)
            throw new ArgumentOutOfRangeException(nameof(index), "Round index is out of range");

        return shuffledRounds[index];
    }

    private void HookMechanic(IRoundMechanic mechanic)
    {
        mechanic.OnValidInput += HandleValidInput;
        mechanic.OnInvalidInput += HandleInvalidInput;

        currentKeyChangedHandler = keys => OnActiveKeysChanged?.Invoke(keys);
        mechanic.OnCurrentKeyChanged += currentKeyChangedHandler;
    }

    private void UnhookMechanic(IRoundMechanic mechanic)
    {
        mechanic.OnValidInput -= HandleValidInput;
        mechanic.OnInvalidInput -= HandleInvalidInput;

        if (currentKeyChangedHandler != null)
        {
            mechanic.OnCurrentKeyChanged -= currentKeyChangedHandler;
            currentKeyChangedHandler = null;
        }
    }

    public void ResetCurrentRound()
    {
        isActive = false;
        tickable = null;

        inputController.EnableInput(false);
        inputController.Unbind();

        if(currentMechanic != null)
        {
            UnhookMechanic(currentMechanic);
            currentMechanic = null;
        }
    }

    private void HandleValidInput()
    {
        if (!isActive) return;

        if (GameManager.Instance.CurrentState != GameState.InGame)
            return;

        if (GameManager.Instance.IsPaused)
            return;

        CurrentProgress++;

        if (currentMechanic is IProgressAware progressAware)
        {
            progressAware.OnProgressChanged(CurrentProgress, CurrentRequiredTaps);
        }

        OnRoundValidInput?.Invoke();

        if (CurrentProgress >= CurrentRequiredTaps)
            EndRound(true);
    }

    private void HandleInvalidInput()
    {
        if (!isActive) return;

        if (GameManager.Instance.CurrentState != GameState.InGame)
            return;

        if (GameManager.Instance.IsPaused)
            return;

        CurrentProgress = Mathf.Max(0, CurrentProgress - 1);

        if (currentMechanic is IProgressAware progressAware)
        {
            progressAware.OnProgressChanged(CurrentProgress, CurrentRequiredTaps);
        }

        OnRoundInvalidInput?.Invoke();
    }

    private void WinRound() => EndRound(true);
    private void LoseRound() => EndRound(false);   

    private IRoundMechanic CreateMechanic(RoundMechanic type)
    {
        return type switch
        {
            RoundMechanic.SingleKey => new SingleButtonMECH(),
            RoundMechanic.AlternateKeys => new AlternateButtonsMECH(),
            RoundMechanic.SplitKeys => new SplitPhaseMECH(),
            RoundMechanic.KeySequence => new RandomButtonsMECH(),
            RoundMechanic.CorrectKey => new CorrectKeyMECH(),
            _ => throw new System.Exception("Unknown mechanic")
        };
    }
}
