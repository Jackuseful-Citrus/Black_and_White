using UnityEngine;
using TMPro;
using System.Collections;

public class NPCAnimator : MonoBehaviour
{
    [Header("依赖组件")]
    [SerializeField] private InteractablePrompt interactable;
    private Animator npcAnimator;
    
    [Header("动画设置")]
    public string animationTriggerParameter = "IsActive";
    
    [Header("对话设置")]
    [SerializeField] private KeyCode talkKey = KeyCode.E;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    public string[] dialogueLines = {
        "find three hidden treasures in the xxx.",
        "I don't trust strangers easily",
        "Try go through the different paths by the door"
    };
    [SerializeField] private float dialogueDuration = 3f;

    private bool isShowingDialogue = false;
    private int currentLineIndex = 0;

    private void Reset()
    {
        if (interactable == null)
            interactable = GetComponent<InteractablePrompt>();
    }

    void Start()
    {
        npcAnimator = GetComponent<Animator>();

        if (npcAnimator == null)
        {
            Debug.LogWarning("[NPCAnimator] 未找到 Animator 组件");
        }
        else
        {
            npcAnimator.SetBool(animationTriggerParameter, false);
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (interactable == null) return;

        // 更新动画状态
        if (npcAnimator != null)
        {
            npcAnimator.SetBool(animationTriggerParameter, interactable.PlayerInRange);
        }

        // 按 E 键对话
        if (interactable.PlayerInRange && Input.GetKeyDown(talkKey) && !isShowingDialogue)
        {
            ShowNextDialogue();
        }
    }

    private void ShowNextDialogue()
    {
        if (dialogueLines.Length == 0 || dialoguePanel == null) return;

        // 按顺序选择对话
        string selectedLine = dialogueLines[currentLineIndex];
        
        // 移动到下一句（循环）
        currentLineIndex = (currentLineIndex + 1) % dialogueLines.Length;

        // 显示对话
        dialoguePanel.SetActive(true);
        if (dialogueText != null)
            dialogueText.text = selectedLine;

        // 隐藏提示圆圈
        if (interactable != null)
            interactable.SetHintVisible(false);

        isShowingDialogue = true;
        StartCoroutine(HideDialogueAfterDelay());
    }

    private IEnumerator HideDialogueAfterDelay()
    {
        yield return new WaitForSeconds(dialogueDuration);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // 恢复提示圆圈
        if (interactable != null)
            interactable.SetHintVisible(true);

        isShowingDialogue = false;
    }
}