using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;

    [Header("Infos Images")]
    [SerializeField] private Image[] buttonImages;

    [Header("Info Pages")]
    [SerializeField] private GameObject[] infoPages;

    [SerializeField] private float fadeDuration;
    private UIFade fader;
    private bool isFading;

    private void Awake()
    {
        fader = GetComponent<UIFade>();
    }

    private void Start()
    {
        HideAllInfos();
        DimAllButtons();

        ShowPage(0);
    }

    public void ShowFade()
    {
        if (isFading)
            return;

        isFading = true;
        fader.FadeIn(fadeDuration);
    }

    public void HideFade()
    {
        isFading = false;
        fader.FadeOut(fadeDuration);
    }

    public void ShowPage(int index)
    {
        if (index < 0 || index >= infoPages.Length)
        {
            Debug.LogError($"Invalid tutorial index: {index}");
            return;
        }

        HideAllInfos();
        DimAllButtons();

        infoPages[index].SetActive(true);
        buttonImages[index].color = activeColor;
    }

    private void HideAllInfos()
    {
        foreach (var page in infoPages)
            page.SetActive(false);
    }

    private void DimAllButtons()
    {
        foreach (var img in buttonImages)
            img.color = inactiveColor;
    }

    private void OnValidate()
    {
        if (buttonImages.Length != infoPages.Length)
            Debug.LogError("Tutorial UI arrays must be the same length", this);
    }
}
