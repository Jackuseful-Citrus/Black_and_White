using UnityEngine;

/// <summary>
/// Opening-only controller: auto-orbits around a center, keeps feet on the sphere, toggles black/white visuals, and forces walking anim.
/// </summary>
public class OpeningPlayerController : MonoBehaviour
{
    [Header("Orbit")]
    [SerializeField] private Transform center;     // orbit center
    [SerializeField] private float radius = 1.5f;  // orbit radius
    [SerializeField] private float angularSpeed = 90f; // degrees per second
    [SerializeField] private int direction = 1;    // 1 CW, -1 CCW

    [Header("Orientation")]
    [SerializeField] private Transform visualRoot;          // optional: rotate this instead of the root
    [SerializeField] private float visualRotationOffset = 0f; // extra Z-rotation (deg) to fix head/feet alignment

    [Header("Outlooks")]
    [SerializeField] private GameObject blackOutlook;
    [SerializeField] private GameObject whiteOutlook;

    [Header("Animator")]
    [SerializeField] private Animator blackAnimator;
    [SerializeField] private Animator whiteAnimator;
    [SerializeField] private string walkBoolName = "IsWalking";
    [SerializeField] private string switchToWhiteTrigger = "SwitchToWhite";
    [SerializeField] private string switchToBlackTrigger = "SwitchToBlack";

    private float angleDeg;
    private bool isBlack = true; // left hemisphere assumed black
    private int walkHash;

    private bool HasParameter(Animator animator, string paramName)
    {
        if (animator == null || string.IsNullOrEmpty(paramName)) return false;
        foreach (var p in animator.parameters)
        {
            if (p.name == paramName) return true;
        }
        return false;
    }

    private void SetWalkState(Animator animator, bool isMoving)
    {
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) return;
        if (!HasParameter(animator, walkBoolName)) return;
        animator.SetBool(walkHash, isMoving);
    }

    private void Awake()
    {
        walkHash = Animator.StringToHash(walkBoolName);

        // auto-grab animators from outlooks
        if (blackAnimator == null && blackOutlook != null)
            blackAnimator = blackOutlook.GetComponent<Animator>();
        if (whiteAnimator == null && whiteOutlook != null)
            whiteAnimator = whiteOutlook.GetComponent<Animator>();
    }

    private void Start()
    {
        if (center == null)
        {
            Debug.LogError("OpeningPlayerController: center not set");
            enabled = false;
            return;
        }

        Vector2 offset = transform.position - center.position;
        if (offset.sqrMagnitude < 0.0001f)
        {
            offset = Vector2.right * radius;
            transform.position = center.position + (Vector3)offset;
        }

        angleDeg = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        UpdatePose(force: true);
    }

    private void Update()
    {
        if (!isActiveAndEnabled || center == null) return;

        angleDeg += direction * angularSpeed * Time.deltaTime;
        UpdatePose(force: false);

        // Opening: always treat as walking (no idle)
        HandleWalkAnimation(true);
    }

    private void UpdatePose(bool force)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector2 pos = (Vector2)center.position + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        transform.position = pos;

        // Build an orthonormal basis: x=tangent, y=outward, z=normal
        Vector3 outward = ((Vector3)pos - center.position).normalized;
        Vector3 tangent = new Vector3(-outward.y, outward.x, 0f) * direction;
        tangent.Normalize();
        Vector3 normal = Vector3.Cross(tangent, outward).normalized;

        Quaternion rotation = Quaternion.LookRotation(normal, outward);
        if (Mathf.Abs(visualRotationOffset) > 0.001f)
        {
            rotation = Quaternion.AngleAxis(visualRotationOffset, Vector3.forward) * rotation;
        }

        Transform target = visualRoot != null ? visualRoot : transform;
        target.rotation = rotation;
        if (target != transform)
        {
            transform.rotation = rotation; // keep collider root aligned too
        }

        bool nowBlack = pos.x < center.position.x;
        if (force || nowBlack != isBlack)
        {
            SwitchForm(nowBlack);
        }
    }

    private void SwitchForm(bool toBlack)
    {
        isBlack = toBlack;

        if (blackOutlook != null) blackOutlook.SetActive(isBlack);
        if (whiteOutlook != null) whiteOutlook.SetActive(!isBlack);

        // 切到黑：在白形态 Animator 上触发 SwitchToBlack；切到白：在黑形态 Animator 上触发 SwitchToWhite
        if (isBlack)
        {
            if (whiteAnimator != null &&
                whiteAnimator.isActiveAndEnabled &&
                whiteAnimator.runtimeAnimatorController != null &&
                HasParameter(whiteAnimator, switchToBlackTrigger))
            {
                // whiteAnimator.ResetTrigger(switchToBlackTrigger);
                whiteAnimator.SetTrigger(switchToBlackTrigger);
            }
        }
        else
        {
            if (blackAnimator != null && blackAnimator.isActiveAndEnabled && blackAnimator.runtimeAnimatorController != null)
            {
                // blackAnimator.ResetTrigger(switchToWhiteTrigger);
                blackAnimator.SetTrigger(switchToWhiteTrigger);
            }
            if (blackAnimator != null &&
                blackAnimator.isActiveAndEnabled &&
                blackAnimator.runtimeAnimatorController != null &&
                HasParameter(blackAnimator, switchToWhiteTrigger))
            {
                blackAnimator.ResetTrigger(switchToWhiteTrigger);
            }
        }

        // 切完：先清 walk，再立即打开，防止停在静态帧
        SetWalkState(blackAnimator, false);
        SetWalkState(whiteAnimator, false);
        HandleWalkAnimation(true);
    }

    private void HandleWalkAnimation(bool isMoving)
    {
        if (isBlack)
        {
            if (blackAnimator != null && blackAnimator.isActiveAndEnabled && blackAnimator.runtimeAnimatorController != null)
            {
                blackAnimator.SetBool(walkHash, isMoving);
            }
            if (whiteAnimator != null && whiteAnimator.isActiveAndEnabled && whiteAnimator.runtimeAnimatorController != null)
            {
                whiteAnimator.SetBool(walkHash, false);
            }
        }
        else
        {
            if (whiteAnimator != null && whiteAnimator.isActiveAndEnabled && whiteAnimator.runtimeAnimatorController != null)
            {
                whiteAnimator.SetBool(walkHash, isMoving);
            }
            if (blackAnimator != null && blackAnimator.isActiveAndEnabled && blackAnimator.runtimeAnimatorController != null)
            {
                blackAnimator.SetBool(walkHash, false);
            }
        }
    }
}
