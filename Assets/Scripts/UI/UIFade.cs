using UnityEngine;
using System.Collections;

// CanvasGroupを使ってUIのフェード制御を行うクラス
[RequireComponent(typeof(CanvasGroup))]
public class UIFade : MonoBehaviour
{
    private CanvasGroup cg;
    private Coroutine currentFade;

    private void Awake()
    {
        // 同一オブジェクトのCanvasGroup取得
        cg = GetComponent<CanvasGroup>();
    }

    // フェードイン（表示）
    public void FadeIn(float duration = 0.3f)
    {
        cg.alpha = 0f;
        StartFade(1f, duration, true);
    }

    // フェードアウト（非表示）
    public void FadeOut(float duration = 0.3f)
    {
        cg.alpha = 1f;
        StartFade(0f, duration, false);
    }

    // フェード開始処理
    private void StartFade(float target, float duration, bool interactable)
    {
        // 既存フェードがあれば停止
        if (currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(FadeRoutine(target, duration, interactable));
    }

    // フェード本体コルーチン
    private IEnumerator FadeRoutine(float target, float duration, bool interactable)
    {
        // フェード中は操作不可
        cg.blocksRaycasts = false;
        cg.interactable = false;

        float start = cg.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }

        // 最終値を保証
        cg.alpha = target;

        // 完了後の操作可否設定
        cg.blocksRaycasts = interactable;
        cg.interactable = interactable;
    }
}