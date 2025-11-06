using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;      // 子弹预制体
    [SerializeField] private Transform firePoint;          // 发射点
    [SerializeField] private float fireInterval = 2f;      // 发射间隔（秒）
    [SerializeField] private float bulletSpeed = 8f;       // 子弹速度


    private float timer = 0f;

    void Update()
    {

        timer += Time.deltaTime;
        if (timer >= fireInterval)
        {
            Fire();
            timer = 0f;
        }
    }

    void Fire()
    {
        Debug.Log("Enemy Fire");
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = firePoint.right * bulletSpeed;
        }
    }
}
