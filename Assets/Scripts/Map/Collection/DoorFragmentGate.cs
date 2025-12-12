using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Boss 前碎片门：两个方块组成，按 E 交互，集齐碎片则上下分开。
/// - 将脚本挂在门父节点（父节点带 trigger Collider2D），引用 top/bottom 两个方块。
/// - 方块自身 Collider2D 负责挡人；开门时分别按 offset 移动。
/// - 碎片判定依赖 FragmentCollectionManager（需在场景常驻）。
/// - 可选：在 Canvas 显示一个提示 UI（如“按 E”），在提示字段里填入。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DoorFragmentGate : MonoBehaviour
{
    [Header("Blocks")]
    [SerializeField] private Transform topSquare;
    [SerializeField] private Transform bottomSquare;
    [SerializeField] private Vector3 topOpenOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private Vector3 bottomOpenOffset = new Vector3(0f, -2f, 0f);

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool requireAllFragments = true; // true: 3 个都要；false: 只要任意一个即可
    [SerializeField] private List<FragmentId> requiredIds = new List<FragmentId>(); // 若为空且 requireAllFragments=true，则默认检查是否收集数量 >=3
    [SerializeField] private float openDuration = 0.6f;
    [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private GameObject promptUI; // 可选：提示 UI（例如“按 E”）

    private bool playerInside;
    private bool opened;
    private Vector3 topStart;
    private Vector3 bottomStart;
    private Coroutine openRoutine;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Awake()
    {
        if (topSquare != null) topStart = topSquare.position;
        if (bottomSquare != null) bottomStart = bottomSquare.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        SetPrompt(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        SetPrompt(false);
    }

    private void Update()
    {
        if (!playerInside || opened) return;
        if (Input.GetKeyDown(interactKey))
        {
            if (HasRequiredFragments())
            {
                OpenGate();
            }
            else
            {
                // 可在此添加未收集完成的提示
            }
        }
    }

    private bool HasRequiredFragments()
    {
        var mgr = FragmentCollectionManager.Instance;
        if (mgr == null) return false;

        if (requireAllFragments)
        {
            if (requiredIds != null && requiredIds.Count > 0)
            {
                for (int i = 0; i < requiredIds.Count; i++)
                {
                    if (!mgr.IsCollected(requiredIds[i])) return false;
                }
                return true;
            }
            else
            {
                // 默认：需要收集数量 >= 3
                return mgr.CollectedCount >= 3;
            }
        }
        else
        {
            if (requiredIds != null && requiredIds.Count > 0)
            {
                for (int i = 0; i < requiredIds.Count; i++)
                {
                    if (mgr.IsCollected(requiredIds[i])) return true;
                }
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    private void OpenGate()
    {
        if (opened) return;
        opened = true;
        SetPrompt(false);

        if (openRoutine != null) StopCoroutine(openRoutine);
        openRoutine = StartCoroutine(OpenAnim());
    }

    private IEnumerator OpenAnim()
    {
        float timer = 0f;
        float dur = Mathf.Max(0.05f, openDuration);

        Vector3 topEnd = topStart + topOpenOffset;
        Vector3 bottomEnd = bottomStart + bottomOpenOffset;

        while (timer < dur)
        {
            float t = Mathf.Clamp01(timer / dur);
            float u = openCurve != null ? openCurve.Evaluate(t) : t;

            if (topSquare != null)
            {
                topSquare.position = Vector3.Lerp(topStart, topEnd, u);
            }
            if (bottomSquare != null)
            {
                bottomSquare.position = Vector3.Lerp(bottomStart, bottomEnd, u);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (topSquare != null) topSquare.position = topEnd;
        if (bottomSquare != null) bottomSquare.position = bottomEnd;
        openRoutine = null;
    }

    private void SetPrompt(bool show)
    {
        if (promptUI != null)
        {
            promptUI.SetActive(show && !opened);
        }
    }
}
