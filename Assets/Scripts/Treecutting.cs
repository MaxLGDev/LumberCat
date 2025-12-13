using TMPro;
using UnityEngine;

public class Treecutting : MonoBehaviour
{
    [SerializeField] private Vector3 rightPosition;
    [SerializeField] private Vector3 leftPosition;

    public System.Action<int> OnRoundTapChanged;
    public System.Action<int> OnTotalTapChanged;
    public int TotalTap;
    public int RoundTaps = 0;

    public bool inputEnabled = true;
    private KeyCode lastKey;

    private void Update()
    {
        if (!inputEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
            ProcessTap(KeyCode.Q);    
        
        if (Input.GetKeyDown(KeyCode.D))
            ProcessTap(KeyCode.D);
    }

    private void ProcessTap(KeyCode key)
    {
        TotalTap++;
        OnTotalTapChanged?.Invoke(TotalTap);

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
            RoundTaps = Mathf.Max(0, RoundTaps - 1);
        }

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
