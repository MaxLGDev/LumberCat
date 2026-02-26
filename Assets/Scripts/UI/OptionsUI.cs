using UnityEngine;

// オプション画面のフェード表示制御クラス
public class OptionsUI : MonoBehaviour
{
    [SerializeField] private float fadeDuration; // フェード時間
    private UIFade fader;
    private bool isFading;

    private void Awake()
    {
        // 同一オブジェクトのUIFade取得
        fader = GetComponent<UIFade>();
    }

    // フェード表示
    public void ShowFade()
    {
        if (isFading)
            return;

        isFading = true;
        fader.FadeIn(fadeDuration);
    }

    // フェード非表示
    public void HideFade()
    {
        isFading = false;
        fader.FadeOut(fadeDuration);
    }
}