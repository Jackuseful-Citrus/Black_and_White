using System.Collections;
using UnityEngine;

public class WhiteScreenFader : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 1.5f;

    private void Reset()
    {
        // 自动抓同物体上的 CanvasGroup
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        canvasGroup.gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;   // 白屏时挡住输入

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);
            canvasGroup.alpha = k;           // 0 → 1
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    public IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);
            canvasGroup.alpha = 1f - k;      // 1 → 0
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
}
