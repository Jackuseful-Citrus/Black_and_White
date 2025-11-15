using UnityEngine;
using System.Collections;

public class ScytheScript : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] GameObject AimPoint;
    [SerializeField] float rotationSpeed = 360f; // 每秒最大旋转角度（度/秒）
    public GameObject Blade;
    private bool isWaiting = false;

    private void Awake()
    {
        // 使用全局 InputManager 实例
        var actions = InputManager.Instance.PlayerInputActions;
        actions.Player.Attack.performed += ctx =>
        {
            if (isWaiting) return;
            isWaiting = true;
            StartCoroutine(Attack());
        };
    }

    private IEnumerator Attack()
    {
        // 这里放镰刀攻击动画效果
        yield return new WaitForSeconds(0.05f); // 攻击前摇
        if (Blade != null) Blade.SetActive(true);
        yield return new WaitForSeconds(0.1f); // 刀锋碰撞箱显示
        if (Blade != null) Blade.SetActive(false);
        yield return new WaitForSeconds(0.2f); // 攻击后摇
        isWaiting = false;
    }

    private void Update()
    {
        if (Player != null) transform.position = Player.transform.position;
        if (AimPoint == null) return;

        Vector3 aimPos = AimPoint.transform.position;
        Vector3 dir = aimPos - transform.position;
        if (dir.sqrMagnitude <= 0f) return;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float currentAngle = transform.rotation.eulerAngles.z;
        float maxDelta = rotationSpeed * Time.deltaTime;
        float delta = Mathf.DeltaAngle(currentAngle, targetAngle); // 最短角度差（-180..180）
        float clamped = Mathf.Clamp(delta, -maxDelta, maxDelta);
        float newAngle = currentAngle + clamped;
        transform.rotation = Quaternion.AngleAxis(newAngle, Vector3.forward);
    }

    private void OnDisable()
    {
        // 脚本禁用时注销事件
        var actions = InputManager.Instance?.PlayerInputActions;
        if (actions != null)
        {
            actions.Player.Attack.performed -= ctx =>
            {
                if (isWaiting) return;
                isWaiting = true;
                StartCoroutine(Attack());
            };
        }
    }
}
