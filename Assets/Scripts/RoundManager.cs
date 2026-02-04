using System;
using UnityEngine;

public enum RoundMechanic
{
    SingleButton,
    AlternateButtons,
    SplitPhase,
    ButtonSequence,
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
}

public class RoundManager : MonoBehaviour
{
    public event Action OnRoundStarted;
    public event Action OnRoundValidInput;
    public event Action OnRoundInvalidInput;
    public event Action<bool> OnRoundEnded;
    public event Action<KeyCode[]> OnActiveKeysChanged;

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

    public IRoundMechanic CurrentMechanic => currentMechanic;

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
        float t = (float)(roundIndex + 1) / shuffledRounds.Length;

        int taps = Mathf.RoundToInt(Mathf.Lerp(baseTaps, maxTaps, t));
        float time = Mathf.Lerp(baseTime, minTime, t);
        float scaledTime = UnityEngine.Random.Range(time * 0.7f, time);

        CurrentRequiredTaps = taps;
        CurrentTaps = 0;

        currentRound = round;
        currentRound.duration = scaledTime;

        currentMechanic = CreateMechanic(round.mechanic);
        tickable = currentMechanic as ITickable;
        HookMechanic(currentMechanic);

        currentMechanic.StartRound(taps, round.allowedKeys);

        inputController.Bind(currentMechanic.HandleKey);
        inputController.EnableInput(false);

        OnRoundStarted?.Invoke();
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
        mechanic.OnCompleted += WinRound;

        mechanic.OnCurrentKeyChanged += keys => OnActiveKeysChanged?.Invoke(keys);
    }

    private void UnhookMechanic(IRoundMechanic mechanic)
    {
        mechanic.OnValidInput -= HandleValidInput;
        mechanic.OnInvalidInput -= HandleInvalidInput;
        mechanic.OnCompleted -= WinRound;
    }


    private void HandleValidInput()
    {
        CurrentTaps++;
        OnRoundValidInput?.Invoke();
    }

    private void HandleInvalidInput()
    {
        OnRoundInvalidInput?.Invoke();
    }

    private void WinRound() => EndRound(true);
    private void LoseRound() => EndRound(false);   

    private IRoundMechanic CreateMechanic(RoundMechanic type)
    {
        return type switch
        {
            RoundMechanic.SingleButton => new SingleButtonMECH(),
            RoundMechanic.AlternateButtons => new AlternateButtonsMECH(),
            RoundMechanic.SplitPhase => new SplitPhaseMECH(),
            RoundMechanic.ButtonSequence => new RandomButtonsMECH(),
            RoundMechanic.CorrectKey => new CorrectKeyMECH(),
            _ => throw new System.Exception("Unknown mechanic")
        };
    }
}
