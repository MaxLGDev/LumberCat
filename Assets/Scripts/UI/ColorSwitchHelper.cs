using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

// 言語切替と表示色更新を行うクラス
public class ColorSwitchHelper : MonoBehaviour
{
    // 対応言語
    private enum Language { English, Japanese }
    private Language activeLanguage;

    [Header("Colors")]
    [SerializeField] private Color activeColor;     // 選択中カラー
    [SerializeField] private Color inactiveColor;   // 非選択カラー

    [Header("Texts")]
    [SerializeField] private TMP_Text englishText;
    [SerializeField] private TMP_Text japaneseText;

    [Header("Buttons")]
    [SerializeField] private Button englishButton;
    [SerializeField] private Button japaneseButton;

    private void Start()
    {
        // 現在のロケールから初期言語判定
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;
        activeLanguage = code.StartsWith("ja")
            ? Language.Japanese
            : Language.English;

        UpdateVisuals();

        // ロケール変更イベント登録
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

        // ボタン登録
        englishButton.onClick.AddListener(() => SetLanguage(Language.English));
        japaneseButton.onClick.AddListener(() => SetLanguage(Language.Japanese));
    }

    // ロケール変更時に呼ばれる
    private void OnLocaleChanged(UnityEngine.Localization.Locale newLocale)
    {
        activeLanguage = newLocale.Identifier.Code.StartsWith("ja")
            ? Language.Japanese
            : Language.English;

        UpdateVisuals();
    }

    // 言語変更処理
    private void SetLanguage(Language desiredLanguage)
    {
        if (desiredLanguage == activeLanguage)
            return; // 既に選択中

        var localeCode = desiredLanguage == Language.Japanese ? "ja-JP" : "en-US";

        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        LocalizationSettings.SelectedLocale = locale;
    }

    // 選択中言語に応じて色更新
    private void UpdateVisuals()
    {
        if (activeLanguage == Language.Japanese)
        {
            japaneseText.color = activeColor;
            englishText.color = new Color(
                inactiveColor.r,
                inactiveColor.g,
                inactiveColor.b,
                100f / 255f); // 半透明
        }
        else
        {
            englishText.color = activeColor;
            japaneseText.color = new Color(
                inactiveColor.r,
                inactiveColor.g,
                inactiveColor.b,
                100f / 255f); // 半透明
        }
    }

    private void OnDestroy()
    {
        // イベント解除
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }
}