using UnityEngine;

public class ColorSwitch : MonoBehaviour
{
    public enum SwitchColor { White, Black }
    
    [Header("Settings")]
    [SerializeField] private SwitchColor requiredColor;
    [SerializeField] private DoorController targetDoor;
    [SerializeField] private bool isOneShot = false; // If true, can only be used once
    
    private bool hasBeenUsed = false;

    private void ActivateSwitch()
    {
        if (isOneShot && hasBeenUsed) return;

        if (targetDoor != null)
        {
            targetDoor.ToggleDoor();
            hasBeenUsed = true;
            Debug.Log($"[ColorSwitch] Activated by {requiredColor} attack.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check for Blade (Black Attack)
        if (other.GetComponent<BladeScript>() != null)
        {
            if (requiredColor == SwitchColor.Black)
            {
                ActivateSwitch();
            }
            else
            {
                 Debug.Log($"[ColorSwitch] Wrong attack! Required: {requiredColor}, Hit by Blade (Black)");
            }
            return;
        }

        // Check for LightBall (White Attack)
        if (other.GetComponent<LightBallScript>() != null)
        {
            if (requiredColor == SwitchColor.White)
            {
                ActivateSwitch();
            }
            else
            {
                 Debug.Log($"[ColorSwitch] Wrong attack! Required: {requiredColor}, Hit by LightBall (White)");
            }
            return;
        }
    }
}
