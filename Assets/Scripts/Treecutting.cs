using UnityEngine;

public enum RoundMechanic
{
    AlternateButtons,
    SingleButton,
    SplitPhase
}

public class Treecutting : MonoBehaviour
{
    [SerializeField] private Transform hammerSprite;
    [SerializeField] private Transform idlePose;
    [SerializeField] private Transform swingPose;

    public System.Action<int> OnValidTap;
    public System.Action<int> OnInvalidTap;

    public int RoundTaps = 0;

    private bool isSwinging;

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

    private void SetHammerState(bool swinging)
    {
        isSwinging = swinging;

        hammerSprite.SetPositionAndRotation(
            swinging ? swingPose.position : idlePose.position,
            swinging ? swingPose.rotation : idlePose.rotation
            );
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
        bool correct = key != lastKey;  

        if(correct)
        {
            lastKey = key;
            RoundTaps++;
            if (key == KeyCode.Q)
                SetHammerState(true);
            else
                SetHammerState(false);

            OnValidTap?.Invoke(1);
        }
        else
        {
            Penalize();
            OnInvalidTap?.Invoke(1);
        }
    }

    private void ToggleHammerPosition()
    {
        SetHammerState(!isSwinging);
    }


    private void HandleSingleButton()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        ToggleHammerPosition();

        OnValidTap?.Invoke(1);
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
            ToggleHammerPosition();
            RegisterSingleTap();
        }
        else
            Penalize();
    }

    private void RegisterSingleTap()
    {
        OnValidTap?.Invoke(1);
    }

    private void Penalize()
    {
        RoundTaps = Mathf.Max(0, RoundTaps - 1);
    }

    public void ResetRound()
    {
        RoundTaps = 0;
        lastKey = KeyCode.None;
    }
}
