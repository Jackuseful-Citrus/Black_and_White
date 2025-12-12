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

    [Header("Light Settings")]
    [SerializeField] private CandleLight candleLight;
    [SerializeField] private float inactiveMin = 1.5f;
    [SerializeField] private float inactiveMax = 1.5f; // No flicker
    [SerializeField] private float activeMin = 4.0f;
    [SerializeField] private float activeMax = 5.0f;

    private bool isActivated = false;

    private void Reset()
    {
        if (interactable == null)
            interactable = GetComponent<InteractablePrompt>();
        if (candleLight == null)
            candleLight = GetComponentInChildren<CandleLight>();
    }

    private void Start()
    {
        if (successTextPanel != null) successTextPanel.SetActive(false);
        if (candleLight == null) candleLight = GetComponentInChildren<CandleLight>();

        // Check if this torch is the current respawn point
        if (LogicScript.Instance != null)
        {
            float dist = Vector3.Distance(transform.position, LogicScript.Instance.GetTeleportPoint());
            if (dist < 0.5f)
            {
                isActivated = true;
            }
        }
        UpdateLightState();
    }

    private void UpdateLightState()
    {
        if (candleLight != null)
        {
            if (isActivated)
            {
                candleLight.minIntensity = activeMin;
                candleLight.maxIntensity = activeMax;
            }
            else
            {
                candleLight.minIntensity = inactiveMin;
                candleLight.maxIntensity = inactiveMax;
            }
        }
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
                
                isActivated = true;
                UpdateLightState();

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
