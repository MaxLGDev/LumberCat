using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class ColorSwitchHelper : MonoBehaviour
{
    private enum Language { English, Japanese }
    private Language activeLanguage;

    [Header("Colors")]
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;

    [Header("Texts")]
    [SerializeField] private TMP_Text englishText;
    [SerializeField] private TMP_Text japaneseText;

    [Header("Buttons")]
    [SerializeField] private Button englishButton;
    [SerializeField] private Button japaneseButton;

    private void Start()
    {
        // Set initial language based on current locale
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;
        activeLanguage = code.StartsWith("ja") ? Language.Japanese : Language.English;

        UpdateVisuals();

        // Listen for changes in locale
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

        // Hook up buttons
        englishButton.onClick.AddListener(() => SetLanguage(Language.English));
        japaneseButton.onClick.AddListener(() => SetLanguage(Language.Japanese));
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale newLocale)
    {
        activeLanguage = newLocale.Identifier.Code.StartsWith("ja") ? Language.Japanese : Language.English;
        UpdateVisuals();
    }

    private void SetLanguage(Language desiredLanguage)
    {
        if (desiredLanguage == activeLanguage) return; // already active

        var localeCode = desiredLanguage == Language.Japanese ? "ja-JP" : "en-US";
        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        LocalizationSettings.SelectedLocale = locale;
    }

    private void UpdateVisuals()
    {
        if (activeLanguage == Language.Japanese)
        {
            japaneseText.color = activeColor;
            englishText.color = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 100f / 255f);
        }
        else
        {
            englishText.color = activeColor;
            japaneseText.color = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 100f / 255f);
        }
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }
}
