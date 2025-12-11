using UnityEngine;

public class ColorSwitch : MonoBehaviour
{
    public enum SwitchColor { White, Black }
    
    [Header("Settings")]
    [SerializeField] private SwitchColor requiredColor;
    [SerializeField] private DoorController targetDoor;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool isOneShot = false; // If true, can only be used once
    
    private bool isPlayerInRange = false;
    private PlayerControl playerControl;
    private bool hasBeenUsed = false;

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            TryActivateSwitch();
        }
    }

    private void TryActivateSwitch()
    {
        if (isOneShot && hasBeenUsed) return;
        if (playerControl == null) return;

        bool isCorrectColor = false;
        if (requiredColor == SwitchColor.White && playerControl.isWhite) isCorrectColor = true;
        if (requiredColor == SwitchColor.Black && playerControl.isBlack) isCorrectColor = true;

        if (isCorrectColor)
        {
            if (targetDoor != null)
            {
                targetDoor.ToggleDoor();
                hasBeenUsed = true;
                Debug.Log($"[ColorSwitch] Activated by {requiredColor} player.");
            }
        }
        else
        {
            Debug.Log($"[ColorSwitch]Required: {requiredColor}, Player is White: {playerControl.isWhite}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerControl = other.GetComponent<PlayerControl>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerControl = null;
        }
    }
}
