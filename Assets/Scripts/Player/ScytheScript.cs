using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class ScytheScript : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] GameObject AimPoint;
    [SerializeField] float rotationSpeed = 360f;
    public GameObject Blade;
    private bool isWaiting = false;
    private PlayerControl playerControl;

    private void Start()
    {
        // 从 Player 物体上获取 PlayerControl 脚本
        playerControl = Player?.GetComponent<PlayerControl>();
        if (playerControl == null)
        {
            Debug.LogError("[ScytheScript] Player 物体上未找到 PlayerControl 脚本！");
        }
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.05f);
        if (Blade != null) Blade.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        if (Blade != null) Blade.SetActive(false);
        yield return new WaitForSeconds(0.15f);
        isWaiting = false;
    }

    private void Update()
    {
        if (playerControl.isBlack && !isWaiting && playerControl.isAttacking)
            {
                isWaiting = true;
                StartCoroutine(Attack());
            }

        if (Player != null) transform.position = Player.transform.position;
        if (AimPoint == null) return;

        Vector3 aimPos = AimPoint.transform.position;
        Vector3 dir = aimPos - transform.position;
        if (dir.sqrMagnitude <= 0f) return;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float currentAngle = transform.rotation.eulerAngles.z;
        float maxDelta = rotationSpeed * Time.deltaTime;
        float delta = Mathf.DeltaAngle(currentAngle, targetAngle);
        float clamped = Mathf.Clamp(delta, -maxDelta, maxDelta);
        float newAngle = currentAngle + clamped;
        transform.rotation = Quaternion.AngleAxis(newAngle, Vector3.forward);
    }
}
