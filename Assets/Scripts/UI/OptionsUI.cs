using UnityEngine;

public class OptionsUI : MonoBehaviour
{
    [SerializeField] private float fadeDuration;
    private UIFade fader;
    private bool isFading;

    private void Awake()
    {
        fader = GetComponent<UIFade>();
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
}
