using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

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

    [System.Serializable]
    public class DialogueGroup
    {
        [TextArea(1, 3)]
        public string[] lines;
    }

    [Tooltip("多组对白：每次按 E 播放一整组（组内按顺序自动播放）")]
    public List<DialogueGroup> dialogueGroups = new List<DialogueGroup>()
    {
        new DialogueGroup{ lines = new []{ "find three hidden treasures in the xxx.", "I don't trust strangers easily", "Try go through the different paths by the door" } }
    };

    [Tooltip("每句对白显示时长（秒）")]
    [SerializeField] private float lineDuration = 2.5f;

    [Tooltip("整组播完后面板停留多久再隐藏（秒）")]
    [SerializeField] private float endHoldDuration = 0.2f;

    private bool isPlayingGroup = false;
    private int currentGroupIndex = 0;

    private void Reset()
    {
        if (interactable == null)
            interactable = GetComponent<InteractablePrompt>();
    }

    private void Start()
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

        // 按 E 播放一整组对白
        if (interactable.PlayerInRange && Input.GetKeyDown(talkKey) && !isPlayingGroup)
        {
            PlayNextGroup();
        }
    }

    private void PlayNextGroup()
    {
        if (dialoguePanel == null || dialogueText == null) return;
        if (dialogueGroups == null || dialogueGroups.Count == 0) return;

        // 找到一个有效的组（防止某组为空）
        int safety = 0;
        while (safety < dialogueGroups.Count)
        {
            var group = dialogueGroups[currentGroupIndex];
            if (group != null && group.lines != null && group.lines.Length > 0)
                break;

            currentGroupIndex = (currentGroupIndex + 1) % dialogueGroups.Count;
            safety++;
        }

        var selectedGroup = dialogueGroups[currentGroupIndex];
        if (selectedGroup == null || selectedGroup.lines == null || selectedGroup.lines.Length == 0) return;

        // 下一次播放下一组（循环）
        currentGroupIndex = (currentGroupIndex + 1) % dialogueGroups.Count;

        // 开播
        StartCoroutine(PlayGroupCoroutine(selectedGroup.lines));
    }

    private IEnumerator PlayGroupCoroutine(string[] lines)
    {
        isPlayingGroup = true;

        // 显示对话面板
        dialoguePanel.SetActive(true);

        // 隐藏提示圆圈
        if (interactable != null)
            interactable.SetHintVisible(false);

        // 逐句播放
        for (int i = 0; i < lines.Length; i++)
        {
            dialogueText.text = lines[i];
            yield return new WaitForSeconds(lineDuration);
        }

        // 组播完稍微停一下（可选）
        if (endHoldDuration > 0f)
            yield return new WaitForSeconds(endHoldDuration);

        // 隐藏对话
        dialoguePanel.SetActive(false);

        // 恢复提示圆圈
        if (interactable != null)
            interactable.SetHintVisible(true);

        isPlayingGroup = false;
    }
}
