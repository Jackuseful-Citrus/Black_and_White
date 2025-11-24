using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class AimPointScript : MonoBehaviour
{
    private Vector2 mousePosition;

    void Update()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 1f));
    }
}
