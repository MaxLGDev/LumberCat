using UnityEngine;
using UnityEngine.EventSystems;

// UI要素にホバー時の拡大演出を付けるクラス
public class ZoomEffectHelper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;

    private void Awake()
    {
        // 元のスケールを保持
        originalScale = transform.localScale;
    }

    // マウスが乗ったとき
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * 1.1f; // 10%拡大
    }

    // マウスが離れたとき
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale; // 元に戻す
    }
}