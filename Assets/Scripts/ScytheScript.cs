using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScytheScript : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] GameObject AimPoint;
    [SerializeField] float rotationSpeed = 360f; // 每秒最大旋转角度（度/秒）
    private PlayerInputActions playerInputActions;
    public GameObject Blade;
    private bool isWaiting = false;

    private void Start()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Attack.performed += ctx => {
            if (isWaiting) return;
            isWaiting = true;
            StartCoroutine(Attack());
        };
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.05f); // 攻击前摇
        Blade.SetActive(true);
        yield return new WaitForSeconds(0.1f); // 刀锋碰撞箱显示
        Blade.SetActive(false);
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

}
