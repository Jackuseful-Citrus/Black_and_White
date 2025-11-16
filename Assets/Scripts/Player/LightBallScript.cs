using UnityEngine;
using UnityEngine.InputSystem;

public class LightBallScript : MonoBehaviour
{
    [SerializeField] private float acceleration = 5f;
    private Vector3 launchDir = Vector3.zero;
    private float maxSpeed = 5f;
    private float speed = 0f;
    private float timer = 0f;
    private Vector3 moveDir = Vector3.zero;
    private Vector3 worldPos = Vector3.zero;
    private System.Action<InputAction.CallbackContext> mouseCallback; // 保存回调

    private void Start()
    {
        var actions = InputManager.Instance?.PlayerInputActions;
        if (actions == null)
        {
            Debug.LogError("[LightBallScript] InputManager not initialized!");
            return;
        }

        // 创建并保存回调
        mouseCallback = ctx =>
        {
            Vector2 mouseScreenPos = ctx.ReadValue<Vector2>();
            worldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0f)
            );
        };

        actions.Player.MousePosition.performed += mouseCallback;
    }

    private void Update()
    {
        launchDir = (worldPos - transform.position).normalized;
        
        timer += Time.deltaTime;
        if (timer < 2f)
        {
            speed += acceleration * Time.deltaTime;
            speed = Mathf.Min(speed, maxSpeed);
            moveDir = launchDir;
        }
        transform.position += moveDir * speed * Time.deltaTime;
        
        if (timer >= 5f)
        {
            UnsubscribeAndDestroy();
        }
    }

    private void UnsubscribeAndDestroy()
    {
        // 注销事件
        var actions = InputManager.Instance?.PlayerInputActions;
        if (actions != null && mouseCallback != null)
        {
            actions.Player.MousePosition.performed -= mouseCallback;
        }
        
        // 销毁物体
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        // 备用清理：如果物体被其他方式禁用
        var actions = InputManager.Instance?.PlayerInputActions;
        if (actions != null && mouseCallback != null)
        {
            actions.Player.MousePosition.performed -= mouseCallback;
        }
    }
}
