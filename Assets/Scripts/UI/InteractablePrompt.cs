using UnityEngine;

public class InteractablePrompt : MonoBehaviour
{
    [Header("提示圆圈")]
    [SerializeField] private GameObject interactionIcon; 

    public bool PlayerInRange { get; private set; } = false;

    private void Start()
    {
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
    }

    /// 供外部控制圆圈显示/隐藏
    public void SetHintVisible(bool visible)
    {
        if (interactionIcon != null)
        {
            interactionIcon.SetActive(visible && PlayerInRange);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerInRange = true;

        SetHintVisible(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerInRange = false;

        SetHintVisible(false);
    }
}

