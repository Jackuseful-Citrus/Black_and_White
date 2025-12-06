using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeScript : MonoBehaviour
{
    [SerializeField] private float damage = 20f;
    private CapsuleCollider2D capsuleCollider2D;
    private void Start()
    {
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        capsuleCollider2D.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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
    }
    
}
