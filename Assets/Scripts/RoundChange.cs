using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RoundChange : MonoBehaviour
{
    [SerializeField] private Treecutting treecutting;
    [SerializeField] private TMP_Text roundCount;

    [Header("Countdown Details")]
    [SerializeField] private TMP_Text countdownText;
    private int cdTimer = 3;

    [Header("Round Details")]
    [SerializeField] private TMP_Text roundTimer;
    public int RoundNumber { get; private set; }
    //private int maxRound = 10;
    private float roundDuration = 15;
    private float currentTimer;
    private float baseRequiredTap = 20;
    private float requiredTapThisRound;
    private bool roundActive;
    private float currentProgress;

    [Header("Progress Bar")]
    [SerializeField] private Slider progressBar;

    private void Start()
    {
        roundCount.text = $"Round: 1/10";
        RoundNumber = 1;

        StartCoroutine(RoundCountdown());
    }

    private void Update()
    {
        GetTimerDuration();
    }

    private void GetTimerDuration()
    {
        if (roundActive)
        {
            progressBar.value = Mathf.Clamp(treecutting.TotalTap, 0, requiredTapThisRound);

            currentTimer = Mathf.Max(0, currentTimer - Time.deltaTime);
            roundTimer.text = $"Time: {currentTimer:F3}";

            if (treecutting.TotalTap >= requiredTapThisRound)
            {
                roundActive = false;
                treecutting.inputEnabled = false;
                Debug.Log("Round complete!");
            }

            if (currentTimer <= 0)
            {
                roundActive = false;
                treecutting.inputEnabled = false;
                Debug.Log("You failed!");
            }
        }
    }

    private void SetProgress()
    {
        progressBar.minValue = 0;
        progressBar.maxValue = requiredTapThisRound;
        currentProgress = 0;
        treecutting.TotalTap = currentProgress;
    }

    private IEnumerator RoundCountdown()
    {
        requiredTapThisRound = 100; //baseRequiredTap * Mathf.Pow(1.5f, RoundNumber - 1);
        SetProgress();

        roundActive = false;
        treecutting.inputEnabled = false;
        countdownText.gameObject.SetActive(true);

        for (int i = cdTimer; i > 0; i--)
        {
            countdownText.text = $"{i}...";
            yield return new WaitForSeconds(1);
        }

        countdownText.text = $"START!";
        currentTimer = 5; //roundDuration - (RoundNumber - 1) * 1f;

        yield return new WaitForSeconds(0.5f);
        roundActive = true;
        countdownText.gameObject.SetActive(false);

        treecutting.inputEnabled = true;
        // start round
    }


}
