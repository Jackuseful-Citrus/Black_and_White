using UnityEngine;
using UnityEngine.InputSystem;

public class AimPointScript : MonoBehaviour
{
    private Vector2 mousePosition;

    private void Update()
    {
        mousePosition = Mouse.current.position.ReadValue();
        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 1f));
    }
}
