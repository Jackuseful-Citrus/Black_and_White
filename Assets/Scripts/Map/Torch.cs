using UnityEngine;

public class Torch : MonoBehaviour
{
    private bool isPlayerInRange = false;

    private void Update()
    {
        // E 键交互
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (LogicScript.Instance != null)
            {
                LogicScript.Instance.SetRespawnPoint(transform.position);
                Debug.Log("Torch activated! Respawn point set.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player near torch. Press 'E' to interact.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}
