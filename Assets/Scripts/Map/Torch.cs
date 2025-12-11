using UnityEngine;
using System.Collections;
using TMPro;

public class Torch : MonoBehaviour
{
    [SerializeField] private InteractablePrompt interactable;
    [Header("成功提示")]
    [SerializeField] private GameObject successTextPanel; 
    [SerializeField] private TextMeshProUGUI successText; 
    [SerializeField] private string successMessage = "Torch activated!";

    private void Reset()
    {
        if (interactable == null)
            interactable = GetComponent<InteractablePrompt>();
    }

    private void Start()
    {
        if (successTextPanel != null) successTextPanel.SetActive(false);
    }

    private void Update()
    {
        if (interactable == null) return;
        if (!interactable.PlayerInRange) return;

        // E 键交互
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (LogicScript.Instance != null)
            {
                LogicScript.Instance.SetRespawnPoint(transform.position);
                Debug.Log("Torch activated! Respawn point set.");

                LogicScript.Instance.ClearAllBars();
                LogicScript.Instance.RefreshEnemies();
                
                if (successTextPanel != null)
                {
                    successTextPanel.SetActive(true);
                    if (successText != null) successText.text = successMessage;
                    StartCoroutine(HideTextAfter3Seconds());
                }
            }
        }
    }

    private IEnumerator HideTextAfter3Seconds()
    {
        yield return new WaitForSeconds(3f);
        if (successTextPanel != null) successTextPanel.SetActive(false);
    }
}
