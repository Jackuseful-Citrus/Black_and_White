using UnityEngine;
using System.Collections;

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

    private void Awake()
    {
        TryResolvePlayer();
    }

    private void Start()
    {
        if (usePlayerInput)
        {
            TryResolvePlayer();
            if (playerControl == null)
            {
                Debug.LogError("[ScytheScript] PlayerControl not found on Player.");
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
        TryResolvePlayer();

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

    private void TryResolvePlayer()
    {
        if (Player == null)
        {
            var found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) Player = found;
        }
        if (usePlayerInput && playerControl == null && Player != null)
        {
            playerControl = Player.GetComponent<PlayerControl>();
        }
    }
}
