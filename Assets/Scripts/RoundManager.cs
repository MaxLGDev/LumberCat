using UnityEngine;
using UnityEngine.UI;

public enum RoundMechanic
{
    SingleButton,
    AlternateButtons,
    SplitPhase
}

[System.Serializable]
public class RoundDefinition
{
    public RoundMechanic mechanic;
    [Tooltip("Time limit in seconds")]
    public float duration;

    [Tooltip("Taps required to clear the round")]
    public int requiredTaps;

    [Header("UI")]
    public string instructionText;
    public KeyCode[] allowedKeys;

    [TextArea]
    public string description;
}

public class RoundManager : MonoBehaviour
{
    private IRoundMechanic currentMechanic;
    [SerializeField] private InputController inputController;

    [SerializeField] private RoundDefinition[] rounds;
    [SerializeField] private Slider progressBar;

    private int currentTaps;
    private int requiredTaps;
    private int totalTaps;
    private float timer;
    private bool isActive;

    private void Update()
    {
        if (!isActive)
            return;

        if(timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
                LoseRound();
        }
    }

    public void StartRound (RoundDefinition round)
    {
        CleanupMechanic();

        currentMechanic = CreateMechanic(round.mechanic);
        HookMechanic(currentMechanic);
        currentMechanic.StartRound(round.requiredTaps, round.allowedKeys);

        inputController.Bind(currentMechanic.HandleKey);
        inputController.EnableInput(true);

        isActive = true;
        timer = round.duration;
        requiredTaps = round.requiredTaps;
        currentTaps = 0;

        progressBar.minValue = 0f;
        progressBar.maxValue = requiredTaps;
        progressBar.value = 0f;
    }

    private void EndRound()
    {
        inputController.EnableInput(false);
        inputController.Unbind();

        UnhookMechanic(currentMechanic);
        currentMechanic = null;
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

    private void CleanupMechanic()
    {
        if (currentMechanic == null)
            return;

        currentMechanic.OnValidInput -= HandleValidInput;
        currentMechanic.OnInvalidInput -= HandleInvalidInput;
        currentMechanic.OnCompleted -= WinRound;

        currentMechanic = null;
    }

    private void HandleValidInput()
    {
        currentTaps++;
        totalTaps++;
        progressBar.value = currentTaps;
    }

    private void HandleInvalidInput()
    {
        totalTaps++;
        Debug.Log("Wrong input");
    }

    private void WinRound()
    {
        isActive = false;
        CleanupMechanic();
        Debug.Log("Round cleared");
    }

    private void LoseRound()
    {
        isActive = false;
        CleanupMechanic();
        Debug.Log("Round lost");
    }

    private IRoundMechanic CreateMechanic(RoundMechanic type)
    {
        return type switch
        {
            RoundMechanic.SingleButton => new SingleButtonMECH(),
            RoundMechanic.AlternateButtons => new AlternateButtonsMECH(),
            RoundMechanic.SplitPhase => new SplitPhaseMECH(),
            _ => throw new System.Exception("Unknown mechanic")
        };
    }
}
