using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BlackBoss : MonoBehaviour
{
    private float phaseDuration = 0f;
    private float maxHealth = 0f;

    [Header("Charge Movement")]
    [SerializeField] private float blackChargeSpeed = 12f;
    [SerializeField] private float blackRestDuration = 2f;
    [SerializeField] private float minChargeDuration = 0.25f;
    [SerializeField] private float maxChargeDuration = 1.5f;

    [Header("Advanced Difficulty")]
    [SerializeField] private float roamSpeed = 4f;
    [SerializeField] private int maxChainCharges = 2;
    [SerializeField] private float roamChangeInterval = 0.5f;

    [Header("Targeting")]
    [SerializeField] private PlayerControl mainPlayer;
    [SerializeField] private Transform mirrorPlayer;

    [Header("Body (visual)")]
    [SerializeField] private GameObject blackBody;

    [Header("Minion Spawning")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private float minionSpawnOffset = 1.0f;

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

    private Vector2 roamDir;
    private float nextRoamChangeTime;
    private Vector3 arenaCenter;
    private bool isReturning;
    private float chargeStartTime;

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
        restTimer = 0f;
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
                StartReturnToCenter();
            }
        }
        else if (isReturning)
        {
            Vector2 dir = (arenaCenter - transform.position).normalized;
            rb.velocity = dir * (blackChargeSpeed * 0.8f);

            if (Vector2.Distance(transform.position, arenaCenter) < 0.5f)
            {
                StopReturn();
            }
        }
        else
        {
            HandleRoaming();

            restTimer -= Time.fixedDeltaTime;
            if (restTimer <= 0f)
            {
                ExecuteCharge();
            }
        }
    }

    private void HandleRoaming()
    {
        if (Time.time >= nextRoamChangeTime)
        {
            Vector3 targetPos = GetBlackPlayerPosition();
            Vector2 toPlayer = (targetPos - transform.position).normalized;
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            
            roamDir = (toPlayer * 0.7f + randomDir * 0.3f).normalized;
            nextRoamChangeTime = Time.time + roamChangeInterval;
        }
        rb.velocity = roamDir * roamSpeed;
    }

    private void ExecuteCharge()
    {
        Vector3 targetPos = GetBlackPlayerPosition();
        Vector2 dir = (targetPos - transform.position).normalized;
        if (dir == Vector2.zero) dir = Vector2.right;

        chargeDir = dir;
        float estimatedTime = Vector2.Distance(transform.position, targetPos) / Mathf.Max(blackChargeSpeed, 0.01f);
        chargeTimer = Mathf.Clamp(estimatedTime * 1.2f, minChargeDuration, maxChargeDuration);
        isCharging = true;
        chargeStartTime = Time.time;

        SpawnMinions();
    }

    private void SpawnMinions()
    {
        if (minionPrefab == null) return;

        // Spawn 2 minions
        for (int i = 0; i < 2; i++)
        {
            // Random offset around boss
            Vector2 offset = Random.insideUnitCircle.normalized * minionSpawnOffset;
            Vector3 spawnPos = transform.position + (Vector3)offset;

            GameObject minionObj = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
            Enemy enemyScript = minionObj.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                // Force provoke so they target player regardless of color
                enemyScript.SetProvoked(true);
            }
        }
    }

    private void StartReturnToCenter()
    {
        isCharging = false;
        isReturning = true;
    }

    private void StopReturn()
    {
        isReturning = false;
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

        if (isCharging && Time.time - chargeStartTime > 0.1f)
        {
            StartReturnToCenter();
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
        restTimer = 0f;
        phaseStartTime = Time.time;
        phaseEnded = false;
        arenaCenter = transform.position;
        roamDir = Random.insideUnitCircle.normalized;
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
