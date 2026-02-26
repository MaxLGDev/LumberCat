using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// UI要素にホバー・クリック時の色変化演出を付けるクラス
public class HoverColorHelper : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField] private Color normalColor;   // 通常色
    [SerializeField] private Color hoverColor;    // ホバー時色
    [SerializeField] private Color pressedColor;  // 押下時色
    [SerializeField] private float fadeTime = 0.15f; // 色変化時間

    private Graphic[] targets;     // 色変更対象
    private Color currentColor;
    private Color targetColor;
    private bool isHovered;

    [SerializeField] private bool includeParent = true; // 親Graphicを含むか

    private void Awake()
    {
        // 子階層含めてGraphic取得
        var allGraphics = GetComponentsInChildren<Graphic>(true);

        if (includeParent)
            targets = allGraphics;
        else
            targets = Array.FindAll(allGraphics, g => g != GetComponent<Graphic>());

        currentColor = normalColor;
        targetColor = normalColor;

        SetColors(normalColor);
    }

    private void Update()
    {
        // 目標色と一致していれば処理しない
        if (currentColor == targetColor)
            return;

        // 時間ベースで補間（TimeScale無視）
        currentColor = Color.Lerp(
            currentColor,
            targetColor,
            Time.unscaledDeltaTime / fadeTime);

        // 誤差補正
        if (Vector4.Distance(currentColor, targetColor) < 0.001f)
            currentColor = targetColor;

        SetColors(currentColor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        targetColor = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        targetColor = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetColor = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // ホバー状態に応じて色を戻す
        targetColor = isHovered ? hoverColor : normalColor;
    }

    // 対象すべてに色適用
    private void SetColors(Color color)
    {
        foreach (var g in targets)
            g.color = color;
    }

    // 無効化時に状態リセット
    private void OnDisable()
    {
        currentColor = normalColor;
        targetColor = normalColor;
        SetColors(normalColor);
        isHovered = false;
    }
}