using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class LightBallScript : MonoBehaviour
{
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float maxSpeed = 4f;
    private Vector3 launchDir = Vector3.zero;
    private Vector3 velocity = Vector3.zero;
    private float speed = 0f;
    private float journey = 0f;
    private Vector3 moveDir = Vector3.zero;
    private Vector3 mousePos = Vector3.zero;

    private void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // 读取屏幕坐标并使用物体的屏幕深度作为 z
        var mouseScreen = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : (Vector2)Input.mousePosition;
        float depth = cam.WorldToScreenPoint(transform.position).z;
        mousePos = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, depth));

        // 方向（每帧更新）
        launchDir = (mousePos - transform.position).normalized;
        speed = velocity.magnitude;
        // 实际移动
        if (velocity != Vector3.zero)
        {
            transform.position += velocity * Time.deltaTime;
            journey += speed * Time.deltaTime; // 只在有移动时累计路程
        }

        // 加速阶段
        if (journey < 8f)
        {
            if (speed < maxSpeed)
            {
                velocity += moveDir * acceleration * Time.deltaTime;
            }else
            {
                velocity += moveDir * acceleration * Time.deltaTime;
                velocity = velocity.normalized * maxSpeed;
            }

            if (Vector3.Distance(transform.position, mousePos) > 0.2f)
            {
                moveDir = launchDir;
            }
        }

        // 销毁条件
        if (journey >= 16f)
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        
    }
}
