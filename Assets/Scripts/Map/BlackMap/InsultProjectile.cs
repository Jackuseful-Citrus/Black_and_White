using UnityEngine;
using TMPro;

public class InsultProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;
    public float lifeTime = 5f;
    public float knockbackForce = 5f;

    [Header("Text (World Space)")]
    public TMP_Text textMesh;                    // 挂在子物体 Canvas/Text (TMP) 上
    public Vector3 textWorldOffset = new Vector3(0f, 0.8f, 0f);   // 文字相对子弹的世界偏移
    public float hideTextRadius = 0.8f;                           // 离玩家太近时隐藏文字

    [Header("Homing")]
    public bool homing = false;          // 是否有一点点追踪
    public float homingTurnSpeed = 8f;   // 转向速度
    [SerializeField] private float homingDuration = 0.35f; // 追踪时长

    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Transform target;            // 玩家
    private float homingEndTime;

    /// <summary>
    /// 初始化：发射方向 + 文字 + 追踪目标
    /// </summary>
    public void Init(Vector2 direction, string insultText, Transform target = null, bool enableHoming = false)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        moveDir = direction.normalized;
        this.target = target;
        this.homing = enableHoming;

        if (rb != null)
        {
            rb.velocity = moveDir * speed;
        }

        if (textMesh != null)
        {
            textMesh.text = insultText;
            textMesh.enabled = true;
        }

        if (enableHoming && homingDuration > 0f)
            homingEndTime = Time.time + homingDuration;
        else
            homingEndTime = Time.time;

        Destroy(gameObject, lifeTime);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 追踪逻辑
        if (homing && target != null && rb != null && Time.time < homingEndTime)
        {
            Vector2 toTarget = ((Vector2)target.position - rb.position).normalized;
            Vector2 desiredVel = toTarget * speed;

            rb.velocity = Vector2.Lerp(rb.velocity, desiredVel, homingTurnSpeed * Time.deltaTime);
            moveDir = rb.velocity.normalized;
        }

        UpdateTextPosition();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.AddForce(moveDir * knockbackForce, ForceMode2D.Impulse);
            }

            Destroy(gameObject);
        }
    }

    private void UpdateTextPosition()
    {
        if (textMesh == null) return;

        // 根据距离玩家决定要不要显示文字（防止贴在脸上闪一下）
        if (target != null)
        {
            float dist = Vector2.Distance(transform.position, target.position);
            bool tooClose = dist <= hideTextRadius;

            if (tooClose)
            {
                if (textMesh.enabled) textMesh.enabled = false;
                return;
            }
            else if (!textMesh.enabled)
            {
                textMesh.enabled = true;
            }
        }

        // 世界空间：直接跟着子弹走 + 一个小偏移
        textMesh.transform.position = transform.position + textWorldOffset;
    }
}
