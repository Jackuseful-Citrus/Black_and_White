using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeScript : MonoBehaviour
{
    [SerializeField] private float damage = 20f;
    [SerializeField] private float bossDamage = 1f;
    private CapsuleCollider2D capsuleCollider2D;
    private readonly Collider2D[] overlapResults = new Collider2D[8];
    private ContactFilter2D contactFilter;
    private readonly HashSet<Collider2D> hitThisSwing = new HashSet<Collider2D>();
    private void Start()
    {
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        capsuleCollider2D.isTrigger = true;
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(Physics2D.DefaultRaycastLayers);
        contactFilter.useLayerMask = true;
        contactFilter.useTriggers = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[BladeScript] Trigger with {collision.name} (tag={collision.tag}, layer={LayerMask.LayerToName(collision.gameObject.layer)})");

        // 尝试获取 Enemy 组件
        Enemy enemy = collision.GetComponent<Enemy>();
        
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        if (collision.gameObject.CompareTag("WhiteEnemy"))
        {            
            if (LogicScript.Instance != null)
            {
                LogicScript.Instance.BladeHitWhiteEnemy();
            }            
        }
        else if (collision.gameObject.CompareTag("BlackEnemy"))
        {
            if (LogicScript.Instance != null)
            {
                LogicScript.Instance.BladeHitBlackEnemy();
            }
        }

        var blackBoss = collision.GetComponentInParent<BlackBoss>();
        if (blackBoss != null)
        {
            Debug.Log($"[BladeScript] Hit BlackBoss via {collision.name}, dealing {bossDamage}");
            blackBoss.TakeDamage(bossDamage);
        }

        var whiteBoss = collision.GetComponentInParent<WhiteBoss>();
        if (whiteBoss != null)
        {
            Debug.Log($"[BladeScript] Hit WhiteBoss via {collision.name}, dealing {bossDamage}");
            whiteBoss.TakeDamage(bossDamage);
        }
    }

    private void FixedUpdate()
    {
        // 兜底：主动检测叠加，避免 OnTrigger 失效导致无法命中 Boss
        if (!gameObject.activeInHierarchy || capsuleCollider2D == null) return;

        int count = capsuleCollider2D.OverlapCollider(contactFilter, overlapResults);
        for (int i = 0; i < count; i++)
        {
            Collider2D col = overlapResults[i];
            if (col == null || hitThisSwing.Contains(col)) continue;

            hitThisSwing.Add(col);

            Debug.Log($"[BladeScript][Overlap] with {col.gameObject.name} (tag={col.tag}, layer={LayerMask.LayerToName(col.gameObject.layer)})");

            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            var blackBoss = col.GetComponentInParent<BlackBoss>();
            if (blackBoss != null)
            {
                Debug.Log($"[BladeScript][Overlap] Hit BlackBoss via {col.name}, dealing {bossDamage}");
                blackBoss.TakeDamage(bossDamage);
            }

            var whiteBoss = col.GetComponentInParent<WhiteBoss>();
            if (whiteBoss != null)
            {
                Debug.Log($"[BladeScript][Overlap] Hit WhiteBoss via {col.name}, dealing {bossDamage}");
                whiteBoss.TakeDamage(bossDamage);
            }
        }
    }

    private void OnEnable()
    {
        hitThisSwing.Clear();
    }

    private void OnDisable()
    {
        hitThisSwing.Clear();
    }

    public float GetBossDamage()
    {
        return bossDamage;
    }
    
}
