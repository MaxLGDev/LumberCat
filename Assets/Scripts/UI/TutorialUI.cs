using UnityEngine;
using UnityEngine.UI;

// チュートリアル画面のページ切替とフェード制御を行うクラス
public class TutorialUI : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color activeColor;     // 選択中ボタン色
    [SerializeField] private Color inactiveColor;   // 非選択ボタン色

    [Header("Infos Images")]
    [SerializeField] private Image[] buttonImages;  // ページ切替ボタン画像

    [Header("Info Pages")]
    [SerializeField] private GameObject[] infoPages; // 各チュートリアルページ

    [SerializeField] private float fadeDuration;    // フェード時間
    private UIFade fader;
    private bool isFading;

    private void Awake()
    {
        // 同一オブジェクトのUIFade取得
        fader = GetComponent<UIFade>();
    }

    private void Start()
    {
        // 初期化
        HideAllInfos();
        DimAllButtons();

        ShowPage(0); // 最初のページ表示
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

    // 指定ページ表示
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

    // 全ページ非表示
    private void HideAllInfos()
    {
        foreach (var page in infoPages)
            page.SetActive(false);
    }

    // 全ボタンを非選択色へ
    private void DimAllButtons()
    {
        foreach (var img in buttonImages)
            img.color = inactiveColor;
    }

    // 配列サイズチェック（エディタ用）
    private void OnValidate()
    {
        if (buttonImages.Length != infoPages.Length)
            Debug.LogError("Tutorial UI arrays must be the same length", this);
    }
}