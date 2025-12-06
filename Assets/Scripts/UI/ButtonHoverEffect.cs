using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image hoverBlock;
    public Image pointerIcon;
    public float animationDuration = 0.25f;
    public float fallbackTargetWidth = 110f;

    private RectTransform hoverBlockTransform;
    private float targetWidth;
    private bool isInitialized = false;

    void Start()
    {
        hoverBlockTransform = hoverBlock.GetComponent<RectTransform>();
        pointerIcon.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        TryInitialize();
    }

    void TryInitialize()
    {
        if (isInitialized || hoverBlockTransform == null) return;

        Canvas.ForceUpdateCanvases();
        var rt = (RectTransform)transform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        hoverBlockTransform.pivot = new Vector2(0f, hoverBlockTransform.pivot.y);

        float anchorYMin = hoverBlockTransform.anchorMin.y;
        float anchorYMax = hoverBlockTransform.anchorMax.y;
        hoverBlockTransform.anchorMin = new Vector2(0f, anchorYMin);
        hoverBlockTransform.anchorMax = new Vector2(0f, anchorYMax);

        float leftOffset = 10f; 
        hoverBlockTransform.anchoredPosition = new Vector2(leftOffset, hoverBlockTransform.anchoredPosition.y);

        float buttonWidth = ((RectTransform)transform).rect.width;
        float padding = 20f;
        targetWidth = Mathf.Max(0f, buttonWidth - padding);
        if (targetWidth <= 0f) targetWidth = fallbackTargetWidth;

 
        hoverBlockTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);

        isInitialized = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TryInitialize();
        pointerIcon.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AnimateBlock(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerIcon.gameObject.SetActive(false);
        StopAllCoroutines();
        StartCoroutine(AnimateBlock(false));
    }

    private IEnumerator AnimateBlock(bool isHovering)
    {
        float timeElapsed = 0f;
        float startWidth = hoverBlockTransform.rect.width;
        float endWidth = isHovering ? targetWidth : 0f;

        while (timeElapsed < animationDuration)
        {
            float t = timeElapsed / animationDuration;
            float newWidth = Mathf.Lerp(startWidth, endWidth, t);
            hoverBlockTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
            timeElapsed += Time.unscaledDeltaTime; 
            yield return null;
        }

        hoverBlockTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, endWidth);
    }
}
