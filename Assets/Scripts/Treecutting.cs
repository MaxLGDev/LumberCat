using UnityEngine;

public enum RoundMechanic
{
    AlternateButtons,
    SingleButton,
    SplitPhase
}

public class Treecutting : MonoBehaviour
{
    [SerializeField] private Vector3 rightPosition;
    [SerializeField] private Vector3 leftPosition;

    public System.Action<int> OnRoundTapChanged;
    public System.Action<int> OnTotalTapChanged;
    public int TotalTaps;
    public int RoundTaps = 0;

    private int requiredTapsThisRound;

    public bool inputEnabled = true;
    private KeyCode lastKey;

    private RoundMechanic currentMechanic;

    private void Update()
    {
        if (!inputEnabled)
            return;

        switch (currentMechanic)
        {
            case RoundMechanic.AlternateButtons:
                HandleAlternateButtons();
                break;

            case RoundMechanic.SingleButton:
                HandleSingleButton();
                break;

            case RoundMechanic.SplitPhase:
                HandleSplitPhase();
                break;
        }
    }

    public void SetMechanic(RoundMechanic mechanic, int requiredTaps)
    {
        currentMechanic = mechanic;
        requiredTapsThisRound = requiredTaps;
        ResetRound();
    }

    private void HandleAlternateButtons()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            ProcessTap(KeyCode.Q);

        if (Input.GetKeyDown(KeyCode.D))
            ProcessTap(KeyCode.D);
    }

    private void ProcessTap(KeyCode key)
    {
        TotalTaps++;
        OnTotalTapChanged?.Invoke(TotalTaps);

        bool correct = key != lastKey;

        if(correct)
        {
            lastKey = key;
            RoundTaps++;
            if (key == KeyCode.Q)
                GoLeft();
            else
                GoRight();
        }
        else
        {
            Penalize();
        }

        OnRoundTapChanged?.Invoke(RoundTaps);
    }

    private bool spaceTapped = false;

    private void ToggleCatPosition()
    {
        spaceTapped = !spaceTapped;

        if (spaceTapped)
            GoLeft();
        else
            GoRight();
    }


    private void HandleSingleButton()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        ToggleCatPosition();
        TotalTaps++;
        RoundTaps++;
        OnTotalTapChanged?.Invoke(TotalTaps);
        OnRoundTapChanged?.Invoke(RoundTaps);
    }

    private int SplitPoint => requiredTapsThisRound / 2;
    private bool InSecondPhase => RoundTaps >= SplitPoint;

    private void HandleSplitPhase()
    {
        KeyCode pressedKey = KeyCode.None;

        if (Input.GetKeyDown(KeyCode.Q))
            pressedKey = KeyCode.Q;
        else if (Input.GetKeyDown(KeyCode.D))
            pressedKey = KeyCode.D;

        if (pressedKey == KeyCode.None)
            return;

        bool correct =
            (!InSecondPhase && pressedKey == KeyCode.Q) ||
            (InSecondPhase && pressedKey == KeyCode.D);

        if (correct)
        {
            ToggleCatPosition();
            RegisterSingleTap();
        }
        else
            Penalize();


    }

    private void RegisterSingleTap()
    {
        TotalTaps++;
        RoundTaps++;

        OnTotalTapChanged?.Invoke(TotalTaps);
        OnRoundTapChanged?.Invoke(RoundTaps);
    }

    private void Penalize()
    {
        RoundTaps = Mathf.Max(0, RoundTaps - 1);
        OnRoundTapChanged?.Invoke(RoundTaps);
    }

    public void ResetRound()
    {
        RoundTaps = 0;
        lastKey = KeyCode.None;
        OnRoundTapChanged?.Invoke(RoundTaps);
    }

    public void GoLeft() => transform.localPosition = leftPosition;

    public void GoRight() => transform.localPosition = rightPosition;
}
