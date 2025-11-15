using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimPointScript : MonoBehaviour
{
    private void Awake()
    {
        // 使用全局 InputManager 实例
        var actions = InputManager.Instance.PlayerInputActions;

        actions.Player.MousePosition.performed += ctx =>
        {
            Vector2 mouseScreenPos = ctx.ReadValue<Vector2>();
            // 屏幕坐标 → 世界坐标转换
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f)
            );
            transform.position = worldPos;
        };
    }

    private void OnDisable()
    {
        // 脚本禁用时注销事件，避免内存泄漏
        var actions = InputManager.Instance?.PlayerInputActions;
        if (actions != null)
        {
            actions.Player.MousePosition.performed -= ctx =>
            {
                Vector2 mouseScreenPos = ctx.ReadValue<Vector2>();
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(
                    new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f)
                );
                transform.position = worldPos;
            };
        }
    }
}
