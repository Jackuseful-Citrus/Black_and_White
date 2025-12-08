using UnityEngine;

public class Trap : MonoBehaviour
{
    public enum TrapType { Black, White }
    public TrapType trapType;
    
    [Header("Settings")]
    [SerializeField] private float damageInterval = 1f; // 每隔多少秒增加一次数值
    [SerializeField] private int damageAmount = 5;      // 每次增加的数值
    [SerializeField] private float slowMultiplier = 0.5f; // 减速倍率 (0.5 = 50% 速度)

    private bool isPlayerInTrap = false;
    private float timer = 0f;
    private PlayerControl playerControl;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrap = true;
            playerControl = other.GetComponent<PlayerControl>();
            if (playerControl != null)
            {
                playerControl.AddSlowEffect(slowMultiplier);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrap = false;
            if (playerControl != null)
            {
                playerControl.RemoveSlowEffect(slowMultiplier);
                playerControl = null;
            }
            timer = 0f; // 重置计时器
        }
    }

    private void Update()
    {
        if (isPlayerInTrap && LogicScript.Instance != null)
        {
            timer += Time.deltaTime;
            if (timer >= damageInterval)
            {
                timer = 0f;
                if (trapType == TrapType.Black)
                {
                    LogicScript.Instance.AddBlackBar(damageAmount);
                }
                else
                {
                    LogicScript.Instance.AddWhiteBar(damageAmount);
                }
            }
        }
    }
}
