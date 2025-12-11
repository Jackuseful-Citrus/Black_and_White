using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 1.0f;
    [SerializeField] private float openHeight = 0.1f; // The target Y scale when open (almost flat)
    
    private Vector3 initialScale;
    private Vector3 initialPosition;
    private bool isOpen = false;
    private Coroutine currentCoroutine;

    private void Start()
    {
        initialScale = transform.localScale;
        initialPosition = transform.position;
    }

    public void ToggleDoor()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(AnimateDoor(true));
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(AnimateDoor(false));
    }

    private IEnumerator AnimateDoor(bool opening)
    {
        float time = 0;
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.position;

        // Target Scale
        Vector3 targetScale = initialScale;
        if (opening) targetScale.y = openHeight;
        
        float heightDiff = initialScale.y - targetScale.y;
        
        Vector3 targetPos;
        if (opening)
        {
             // Target is compressed.
             // Shift amount from initial position
            float shift = (initialScale.y - openHeight) / 2.0f;
             targetPos = initialPosition - transform.up * shift;
        }
        else
        {
            targetPos = initialPosition;
        }

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = time / animationDuration;
            t = Mathf.SmoothStep(0, 1, t);

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        transform.localScale = targetScale;
        transform.position = targetPos;
    }
}
