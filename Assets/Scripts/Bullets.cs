using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullets : MonoBehaviour
{
    public enum BulletType { White, Black }
    public BulletType bulletType = BulletType.White;
    public float damage = 10f;

    private Rigidbody2D rb;

    void Start()
    {
        //防止与自身相撞
        rb = GetComponent<Rigidbody2D>();
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;  // 初始禁用
            StartCoroutine(EnableColliderAfterDelay(0.1f));  
        }
    }

    private IEnumerator EnableColliderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GetComponent<Collider2D>().enabled = true;
    }

    void Update()
    {
        //子弹旋转贴图方向设置
        if (rb != null && rb.velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg + 180f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Bullet hit: " + collision.gameObject.name);
        
        // 检测是否击中玩家
        PlayerControl player = collision.gameObject.GetComponent<PlayerControl>();
        BloodControl blood = collision.gameObject.GetComponent<BloodControl>();
        
        //对于player的扣血计算
        if (player != null && blood != null)
        {
            Debug.Log("Bullet in" );
            if (player.isWhite && bulletType == BulletType.White)
            {
                blood.AddWhiteMinusBlack(damage);
                Debug.Log($"White hit -> white:{blood.whiteBlood} black:{blood.blackBlood}");
            }
            else if (player.isBlack && bulletType == BulletType.Black)
            {
                blood.AddBlackMinusWhite(damage);
                Debug.Log($"Black hit -> black:{blood.blackBlood} white:{blood.whiteBlood}");
            }
        }
        
        // 检测是否击中敌人
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            // 子弹击中敌人，对敌人造成伤害
            enemy.TakeDamage(damage);
            Debug.Log($"Bullet hit enemy: {collision.gameObject.name}, damage: {damage}");
        }
        
        Destroy(gameObject);
    }
}
