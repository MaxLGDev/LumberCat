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
    [SerializeField] private GameObject countdownCover;
    private int cdTimer = 3;

    [Header("Round Details")]
    [SerializeField] private TMP_Text roundTimer;
    [SerializeField] private TMP_Text tapCounter;
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
        roundCount.text = $"ラウンド: 1/10";
        RoundNumber = 1;

        treecutting.OnTotalTapChanged += HandleTotalTap;
        treecutting.OnRoundTapChanged += HandleRoundTap;

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
            currentTimer = Mathf.Max(0, currentTimer - Time.deltaTime);
            roundTimer.text = $"タイマー: {currentTimer:F3}";

            if (treecutting.RoundTaps >= requiredTapThisRound)
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

    private void HandleRoundTap(int roundTaps)
    {
        progressBar.value = roundTaps;
    }

    private void HandleTotalTap(int totalTaps)
    {
        tapCounter.text = $"タップ: {totalTaps}";
    }

    private void SetProgress()
    {
        treecutting.ResetRound();

        treecutting.RoundTaps = 0;

        progressBar.minValue = 0;
        progressBar.maxValue = requiredTapThisRound;
        progressBar.value = 0f;

        tapCounter.text = $"タップ: {treecutting.TotalTap}";
    }

    private IEnumerator RoundCountdown()
    {
        requiredTapThisRound = baseRequiredTap * Mathf.Pow(1.5f, RoundNumber - 1);
        SetProgress();

        roundActive = false;
        treecutting.inputEnabled = false;
        countdownCover.gameObject.SetActive(true);

        for (int i = cdTimer; i > 0; i--)
        {
            countdownText.text = $"{i}...";
            yield return new WaitForSeconds(1);
        }

        countdownText.text = $"スタート!";
        currentTimer = roundDuration - (RoundNumber - 1) * 1f;

        yield return new WaitForSeconds(0.5f);
        roundActive = true;
        countdownCover.gameObject.SetActive(false);

        treecutting.inputEnabled = true;
        // start round
    }


}
