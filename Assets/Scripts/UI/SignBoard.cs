using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SignBoard : MonoBehaviour
{
    [Header("依赖的可互动提示脚本")]
    [SerializeField] private InteractablePrompt interactable;

    [Header("交互设置")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [TextArea]
    [SerializeField] private string message = "Input your signboard message here.";

    [Header("告示牌 UI")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;

    private bool isShowing = false;
    private bool wasInRange = false;

    private void Reset()
    {
        if (interactable == null)
            interactable = GetComponent<InteractablePrompt>();
    }

    private void Start()
    {
        if (messagePanel != null)
            messagePanel.SetActive(false);
    }

    private void Update()
    {
        if (interactable == null) return;

        // 检测玩家离开区域
        if (wasInRange && !interactable.PlayerInRange)
        {
            if (isShowing)
            {
                CloseMessage();
            }
        }
        wasInRange = interactable.PlayerInRange;

        // 在范围内按 E 切换显示
        if (interactable.PlayerInRange && Input.GetKeyDown(interactKey))
        {
            ToggleMessage();
        }
    }

    private void ToggleMessage()
    {
        isShowing = !isShowing;

        if (messagePanel != null)
            messagePanel.SetActive(isShowing);

        if (isShowing && messageText != null)
            messageText.text = message;

        if (interactable != null)
        {
            interactable.SetHintVisible(!isShowing);
        }
    }

    private void CloseMessage()
    {
        isShowing = false;

        if (messagePanel != null)
            messagePanel.SetActive(false);

        if (interactable != null)
        {
            interactable.SetHintVisible(false);
        }
    }
}
