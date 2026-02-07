using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverColorHelper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Color normalColor;
    [SerializeField] private Color hoverColor;
    [SerializeField] private Color pressedColor;
    [SerializeField] private float fadeTime = 0.15f;

    private Graphic[] targets;
    private Color currentColor;
    private Color targetColor;
    private bool isHovered;

    [SerializeField] private bool includeParent = true;

    private void Awake()
    {
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
        if (currentColor == targetColor)
            return;

        currentColor = Color.Lerp(currentColor, targetColor, Time.unscaledDeltaTime / fadeTime);

        if(Vector4.Distance(currentColor, targetColor) < 0.001f)
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
        targetColor = isHovered ? hoverColor : normalColor;
    }

    private void SetColors(Color color)
    {
        foreach (var g in targets)
            g.color = color;
    }
}
