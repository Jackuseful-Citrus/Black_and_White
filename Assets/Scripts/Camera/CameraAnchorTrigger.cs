using System.Collections;
using UnityEngine;

/// <summary>
/// Player enters trigger -> detach main camera and smoothly move it to a fixed anchor.
/// Works when the camera is normally parented to the player.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CameraAnchorTrigger : MonoBehaviour
{
    [SerializeField] private Transform cameraAnchor; // target position for the camera
    [SerializeField] private float moveDuration = 0.6f;
    [SerializeField] private float targetOrthoSize = 6f;
    [SerializeField] private bool adjustSize = true;
    [SerializeField] private float sizeLerpDuration = 0.6f;

    private Coroutine moveRoutine;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (cameraAnchor == null) return;

        var cam = Camera.main;
        if (cam == null) return;

        cam.transform.SetParent(null);
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }
        moveRoutine = StartCoroutine(SmoothMove(cam.transform));
    }

    private IEnumerator SmoothMove(Transform camTransform)
    {
        Vector3 startPos = camTransform.position;
        Vector3 target = new Vector3(cameraAnchor.position.x, cameraAnchor.position.y, startPos.z);
        float timer = 0f;
        Camera cam = Camera.main;
        float startSize = cam != null ? cam.orthographicSize : 0f;

        while (timer < moveDuration)
        {
            float t = Mathf.Clamp01(timer / Mathf.Max(moveDuration, 0.01f));
            camTransform.position = Vector3.Lerp(startPos, target, t);
            if (adjustSize && cam != null)
            {
                float sizeT = Mathf.Clamp01(timer / Mathf.Max(sizeLerpDuration, 0.01f));
                cam.orthographicSize = Mathf.Lerp(startSize, targetOrthoSize, sizeT);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        camTransform.position = target;
        if (adjustSize && cam != null)
        {
            cam.orthographicSize = targetOrthoSize;
        }
        moveRoutine = null;
    }
}
