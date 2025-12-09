using UnityEngine;

public class Trap : MonoBehaviour
{
    public enum TrapType { Black, White }
    public TrapType trapType;
    
    [Header("Settings")]
    [SerializeField] private float damageInterval = 0.8f; // 每隔多少秒增加一次数值
    //[SerializeField] private int damageAmount = 28;      // 每次增加的数值
    [SerializeField] private float slowMultiplier = 0.2f;

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
            timer = damageInterval - 0.1f; // 退出时稍微延迟一下下次伤害
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
                    LogicScript.Instance.getIntoBlackTrap();
                }
                else
                {
                    LogicScript.Instance.getIntoWhiteTrap();
                }
            }
        }
    }
}
