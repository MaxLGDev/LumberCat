using TMPro;
using UnityEngine;

public class Treecutting : MonoBehaviour
{
    [SerializeField] private Vector3 rightPosition;
    [SerializeField] private Vector3 leftPosition;
    [SerializeField] private TMP_Text tapCounter;

    public bool inputEnabled = true;
    public float TotalTap;
    private KeyCode lastKey;

    private void Start()
    {
        transform.localPosition = rightPosition;
        tapCounter.text = $"Taps: 0";
    }

    private void Update()
    {
        if (!inputEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (lastKey != KeyCode.Q)
            {
                lastKey = KeyCode.Q;
                GoLeft();
            }
            else
            {
                TotalTap = Mathf.Max(0, TotalTap - 1);
            }
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (lastKey != KeyCode.D)
            {
                lastKey = KeyCode.D;
                GoRight();
            }
            else
            {
                TotalTap = Mathf.Max(0, TotalTap - 1);
            }
        }

    }

    public void GoLeft()
    {
        transform.localPosition = leftPosition;
        RegisterTap();
    }

    public void GoRight()
    {
        transform.localPosition = rightPosition;
        RegisterTap();

    }

    private void RegisterTap(int amount = 1)
    {
        TotalTap = Mathf.Max(0, TotalTap + amount);
        tapCounter.text = $"Taps: {TotalTap}";
    }

    [ContextMenu("Reset tap amount")]
    public void ResetTap()
    {
        TotalTap = 0;
        tapCounter.text = $"Taps: 0";
    }
}
