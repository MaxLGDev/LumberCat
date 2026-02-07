using Newtonsoft.Json.Bson;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;


public class ColorSwitchHelper : MonoBehaviour
{
    private enum Language
    {
        English,
        Japanese
    }

    private Language activeLanguage;

    [Header("Colors")]
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;

    [Header("Texts")]
    [SerializeField] private TMP_Text english;
    [SerializeField] private TMP_Text japanese;

    [Header("Icons")]
    [SerializeField] private Image englishIcon;
    [SerializeField] private Image japaneseIcon;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite inactiveSprite;

    private void Start()
    {
        var currentLocale = LocalizationSettings.SelectedLocale.Identifier.Code; // "en" / "ja";
        activeLanguage = currentLocale == "en" ? Language.English : Language.Japanese;

        UpdateButtonVisuals();
    }

    private void UpdateButtonVisuals()
    {

    }

}
