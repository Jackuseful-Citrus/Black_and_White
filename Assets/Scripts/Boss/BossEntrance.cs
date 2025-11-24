using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEntrance : MonoBehaviour
{
    public BossRoomManager bossRoomManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        bossRoomManager.StartBossFight();
        gameObject.SetActive(false); // 只触发一次
    }
}

