using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class UIFade : MonoBehaviour
{
    private CanvasGroup cg;
    private Coroutine currentFade;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    public void FadeIn(float duration = 0.3f)
    {
        cg.alpha = 0f;
        StartFade(1f, duration, true);
    }

    public void FadeOut(float duration = 0.3f)
    {
        cg.alpha = 1f;
        StartFade(0f, duration, false);
    }

    private void StartFade(float target, float duration, bool interactable)
    {
        if(currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(FadeRoutine(target, duration, interactable));
    }

    private IEnumerator FadeRoutine(float target, float duration, bool interactable)
    {
        cg.blocksRaycasts = false;
        cg.interactable = false;

        float start = cg.alpha;
        float t = 0f;

        while(t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }

        cg.alpha = target;
        cg.blocksRaycasts = interactable;
        cg.interactable = interactable;
    }

}
