using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // シェイク前のカメラの初期位置を保持する
    private Vector3 originalPosition;

    private void Awake()
    {
        // ゲーム開始時の位置を基準として保存
        originalPosition = transform.position;
    }

    // カメラシェイク開始
    // duration: 揺れる時間
    // magnitude: 揺れの強さ
    public void Shake(float duration = 0.1f, float magnitude = 0.05f)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    // 指定時間の間、ランダムなオフセットを加えてカメラを揺らす
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // -1〜1の乱数を使ってランダムな揺れを生成
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // 元の位置を基準にオフセットを加算（Z軸は固定）
            transform.position = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // シェイク終了後は必ず元の位置に戻す
        transform.position = originalPosition;
    }
}