using UnityEngine;
using System.Collections;

public class ColorSwitch : MonoBehaviour
{
    public enum SwitchColor { White, Black }
    
    [Header("Settings")]
    [SerializeField] private SwitchColor requiredColor;
    [SerializeField] private DoorController targetDoor;
    [SerializeField] private bool isOneShot = false;
    
    [Header("Visual Feedback")]
    [SerializeField] private Transform rotatingObject; 
    [SerializeField] private float rotationAngle = -120f; 
    [SerializeField] private float rotationDuration = 0.5f; 
    
    private bool hasBeenUsed = false;
    private bool isRotated = false;
    private Coroutine rotationCoroutine;

    private void ActivateSwitch()
    {
        if (isOneShot && hasBeenUsed) return;

        if (targetDoor != null)
        {
            targetDoor.ToggleDoor();
            hasBeenUsed = true;
            Debug.Log($"[ColorSwitch] Activated by {requiredColor} attack.");
        }


        if (rotatingObject != null)
        {
            if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
            
            if (isOneShot)
            {
                rotationCoroutine = StartCoroutine(RotateTo(rotationAngle));
            }
            else
            {
                float targetAngle = isRotated ? 0f : rotationAngle;
                rotationCoroutine = StartCoroutine(RotateTo(targetAngle));
                isRotated = !isRotated;
            }
        }
    }

    private IEnumerator RotateTo(float targetAngle)
    {
        Quaternion startRotation = rotatingObject.localRotation;
        Quaternion endRotation = Quaternion.Euler(0, 0, targetAngle);
        
        float time = 0;
        while (time < rotationDuration)
        {
            time += Time.deltaTime;
            float t = time / rotationDuration;
            t = Mathf.SmoothStep(0, 1, t); 
            
            rotatingObject.localRotation = Quaternion.Lerp(startRotation, endRotation, t);
            yield return null;
        }
        
        rotatingObject.localRotation = endRotation;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
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
