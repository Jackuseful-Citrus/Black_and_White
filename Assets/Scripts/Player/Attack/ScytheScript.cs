using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class ScytheScript : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] GameObject AimPoint;
    [SerializeField] float rotationSpeed = 360f;
    public GameObject Blade;

    [SerializeField] private bool usePlayerInput = true;
    private bool isWaiting = false;
    private PlayerControl playerControl;
    public bool inAttackRecovery = false;
    public bool inAttackProgress = false;

    private void Start()
    {
        // 仅在使用玩家输入时才需要 PlayerControl
        if (usePlayerInput)
        {
            playerControl = Player?.GetComponent<PlayerControl>();
            if (playerControl == null)
            {
                Debug.LogError("[ScytheScript] Player 物体上未找到 PlayerControl 脚本！");
            }
        }
    }

    public void SetAimPoint(GameObject targetAimPoint)
    {
        AimPoint = targetAimPoint;
    }

    public void ForceAttack()
    {
        if (!isWaiting)
        {
            isWaiting = true;
            StartCoroutine(Attack());
        }
    }
    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.05f);
        if (Blade != null) Blade.SetActive(true);
        inAttackProgress = true;
        yield return new WaitForSeconds(0.3f);
        if (Blade != null) Blade.SetActive(false);
        inAttackProgress = false;
        inAttackRecovery = true;
        yield return new WaitForSeconds(0.25f);
        inAttackRecovery = false;
        isWaiting = false;
    }

    private void Update()
    {
        if (usePlayerInput && playerControl != null)
        {
            if (playerControl.isBlack && !isWaiting && playerControl.isAttacking)
            {
                isWaiting = true;
                StartCoroutine(Attack());
            }
        }
        if (Player != null) transform.position = Player.transform.position;
        if (AimPoint == null)
        {
            // 尝试兜底获取场景中的 AimPoint（镜像忘记赋值时仍能旋转）
            AimPoint = FindObjectOfType<AimPointScript>()?.gameObject;
            if (AimPoint == null) return;
        }

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
    private void OnDisable()
    {        
        isWaiting = false;
        inAttackRecovery = false;
        inAttackProgress = false;
        
        if (Blade != null) 
        {
            Blade.SetActive(false);
        }
        StopAllCoroutines();
    }
}
