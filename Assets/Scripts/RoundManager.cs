using System;
using UnityEngine;

public enum RoundMechanic
{
    SingleButton,
    AlternateButtons,
    SplitPhase,
    ButtonSequence
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
    public event Action<RoundDefinition> OnRoundStarted;
    public event Action OnRoundValidInput;
    public event Action OnRoundInvalidInput;
    public event Action<bool> OnRoundEnded;

    private IRoundMechanic currentMechanic;
    [SerializeField] private InputController inputController;
    [SerializeField] private RoundDefinition[] rounds;
    private RoundDefinition currentRound;

    private float timer;
    private bool isActive;

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
    }

    public void PrepareRound(RoundDefinition round)
    {
        currentRound = round;

        currentMechanic = CreateMechanic(round.mechanic);
        HookMechanic(currentMechanic);
        currentMechanic.StartRound(round.requiredTaps, round.allowedKeys);

        inputController.Bind(currentMechanic.HandleKey);
        inputController.EnableInput(false);

        OnRoundStarted?.Invoke(round);
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
        if (index < 0 || index >= rounds.Length)
            throw new ArgumentOutOfRangeException(nameof(index), "Round index is out of range");

        return rounds[index];
    }

    private void HookMechanic(IRoundMechanic mechanic)
    {
        mechanic.OnValidInput += HandleValidInput;
        mechanic.OnInvalidInput += HandleInvalidInput;
        mechanic.OnCompleted += WinRound;
    }

    private void UnhookMechanic(IRoundMechanic mechanic)
    {
        mechanic.OnValidInput -= HandleValidInput;
        mechanic.OnInvalidInput -= HandleInvalidInput;
        mechanic.OnCompleted -= WinRound;
    }


    private void HandleValidInput()
    {
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
            _ => throw new System.Exception("Unknown mechanic")
        };
    }
}
