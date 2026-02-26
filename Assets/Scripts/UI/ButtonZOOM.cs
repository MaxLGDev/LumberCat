using UnityEngine;
using System.Collections;

// ボタン押下時に拡大演出を行い、その後ゲーム開始するクラス
public class ButtonZOOM : MonoBehaviour
{
    [SerializeField] private float targetScale; // 拡大倍率

    // 外部から呼び出す拡大＋クリック処理
    public void SuperZoomAndClick(float duration = 1f)
    {
        StartCoroutine(ZoomCoroutine(duration, targetScale));
    }

    // 拡大アニメーション本体
    private IEnumerator ZoomCoroutine(float duration, float targetSCale)
    {
        Vector3 originalScale = transform.localScale;
        Vector3 finalScale = originalScale * targetScale;

        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            // 補間値は本来 0～1 に正規化すべきだが、
            // 現在は時間をそのまま使用している
            transform.localScale = Vector3.Lerp(originalScale, finalScale, t);

            yield return null;
        }

        // 元のスケールへ戻す
        transform.localScale = originalScale;

        // ゲーム開始
        GameManager.Instance.StartGame();
    }
}