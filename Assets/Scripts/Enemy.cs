using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;  
    [SerializeField] private Transform firePoint;     
    [SerializeField] private float fireInterval = 2f;     
    [SerializeField] private float bulletSpeed = 8f;      


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

        // 计算发射方向（基于firePoint的right方向）
        Vector2 fireDirection = firePoint.right;
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = fireDirection * bulletSpeed;
        }
    }
}
