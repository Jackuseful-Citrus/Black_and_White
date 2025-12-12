using UnityEngine;

/// <summary>
/// Simple contact damage: on collision/trigger with Player, deal damage/notify logic, then optionally destroy.
/// </summary>
public class BlackMapContactDamage : MonoBehaviour
{
    public float damage = 10f;
    public bool destroyOnHit = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }

    private void HandleHit(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        // 可根据项目需要调用具体伤害接口，这里沿用黑敌人命中逻辑
        if (LogicScript.Instance != null)
        {
            LogicScript.Instance.HitByBlackEnemy();
        }

        var rb = col.attachedRigidbody;
        if (rb != null)
        {
            Vector2 knockDir = (col.transform.position - transform.position).normalized;
            rb.AddForce(knockDir * damage * 0.2f, ForceMode2D.Impulse);
        }

        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}
