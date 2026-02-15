using UnityEngine;
using System.Collections;

public class ButtonZOOM : MonoBehaviour
{
    [SerializeField] private float targetScale;

    public void SuperZoomAndClick(float duration = 1f)
    {
        StartCoroutine(ZoomCoroutine(duration, targetScale));
    }

    private IEnumerator ZoomCoroutine(float duration, float targetSCale)
    {
        Vector3 originalScale = transform.localScale;
        Vector3 finalScale = originalScale * targetScale;
        float t = 0f;

        while(t < duration)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(originalScale, finalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
        GameManager.Instance.StartGame();
    }
}
