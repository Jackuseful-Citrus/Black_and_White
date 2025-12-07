using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BlackBoss : MonoBehaviour
{
    private float phaseDuration = 0f;
    private float maxHealth = 0f;

    [Header("Charge Movement")]
    [SerializeField] private float blackChargeSpeed = 10f;
    [SerializeField] private float blackRestDuration = 3f;
    [SerializeField] private float minChargeDuration = 0.25f;
    [SerializeField] private float maxChargeDuration = 1.5f;

    [Header("Targeting")]
    [SerializeField] private PlayerControl mainPlayer;
    [SerializeField] private Transform mirrorPlayer; // optional mirror target

    [Header("Body (visual)")]
    [SerializeField] private GameObject blackBody;

    private Rigidbody2D rb;
    private bool isCharging;
    private float chargeTimer;
    private float restTimer;
    private Vector2 chargeDir;
    private Transform cachedMirror;
    private bool fightActive;
    private float currentHealth;
    private float phaseStartTime;
    private bool phaseEnded;

    public System.Action onPhaseEnded;
    public System.Action onBossDied;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
        restTimer = 0f; // start charging immediately
        if (mainPlayer == null)
        {
            mainPlayer = FindObjectOfType<PlayerControl>();
        }

        if (mirrorPlayer != null)
        {
            cachedMirror = mirrorPlayer;
        }

        if (blackBody != null) blackBody.SetActive(true);

        currentHealth = maxHealth;
        phaseStartTime = Time.time;
    }

    private void FixedUpdate()
    {
        if (!fightActive)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        TickChargeState();
    }

    private void Update()
    {
        if (!fightActive || phaseEnded) return;

        if (phaseDuration > 0f && Time.time - phaseStartTime >= phaseDuration)
        {
            EndPhase(false);
        }
    }

    private void TickChargeState()
    {
        if (isCharging)
        {
            chargeTimer -= Time.fixedDeltaTime;
            rb.velocity = chargeDir * blackChargeSpeed;

            if (chargeTimer <= 0f)
            {
                StopCharge();
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
            restTimer -= Time.fixedDeltaTime;
            if (restTimer <= 0f)
            {
                StartCharge();
            }
        }
    }

    private void StartCharge()
    {
        Vector3 targetPos = GetBlackPlayerPosition();
        Vector2 dir = (targetPos - transform.position).normalized;
        if (dir == Vector2.zero) dir = Vector2.right;

        chargeDir = dir;
        float estimatedTime = Vector2.Distance(transform.position, targetPos) / Mathf.Max(blackChargeSpeed, 0.01f);
        chargeTimer = Mathf.Clamp(estimatedTime, minChargeDuration, maxChargeDuration);
        isCharging = true;
    }

    private void StopCharge()
    {
        isCharging = false;
        rb.velocity = Vector2.zero;
        restTimer = blackRestDuration;
    }

    private Vector3 GetBlackPlayerPosition()
    {
        if (mainPlayer == null)
        {
            mainPlayer = FindObjectOfType<PlayerControl>();
        }

        if (mainPlayer != null && mainPlayer.isBlack)
        {
            return mainPlayer.transform.position;
        }

        if (mirrorPlayer == null && cachedMirror == null)
        {
            cachedMirror = GameObject.FindWithTag("PlayerMirror")?.transform;
        }
        else if (mirrorPlayer != null)
        {
            cachedMirror = mirrorPlayer;
        }

        if (cachedMirror != null)
        {
            return cachedMirror.position;
        }

        return transform.position + Vector3.right;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHitByBlade(collision.otherCollider);

        if (collision.gameObject.CompareTag("Player") && LogicScript.Instance != null)
        {
            LogicScript.Instance.HitByBlackEnemy();
        }

        // end the charge when colliding with any obstacle in the scene
        if (isCharging)
        {
            StopCharge();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHitByBlade(other);
    }

    private void TryHitByBlade(Collider2D col)
    {
        var blade = col.GetComponent<BladeScript>();
        if (blade != null)
        {
            float dmg = blade.GetBossDamage();
            Debug.Log($"[BlackBoss] Hit by Blade ({col.name}), dealing {dmg}");
            TakeDamage(dmg);
        }
    }

    public void BeginFight()
    {
        fightActive = true;
        restTimer = 0f; // immediately start first charge loop
        phaseStartTime = Time.time;
        phaseEnded = false;
    }

    public void PauseFight()
    {
        fightActive = false;
        isCharging = false;
        rb.velocity = Vector2.zero;
    }

    public void ConfigurePhase(float duration, float health)
    {
        phaseDuration = duration;
        maxHealth = health;
        currentHealth = maxHealth;
        phaseStartTime = Time.time;
        phaseEnded = false;
    }

    public void TakeDamage(float amount)
    {
        if (phaseEnded) return;

        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            EndPhase(true);
        }
    }

    public void ForceEndPhase()
    {
        EndPhase(false);
    }

    private void EndPhase(bool wasKilled)
    {
        if (phaseEnded) return;
        phaseEnded = true;
        fightActive = false;
        isCharging = false;
        rb.velocity = Vector2.zero;

        onPhaseEnded?.Invoke();
        if (wasKilled)
        {
            onBossDied?.Invoke();
        }
    }

    public bool HasPhaseEnded => phaseEnded;
}
