using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogicScript : MonoBehaviour
{
    public static LogicScript Instance;

    [SerializeField] GameObject player;
    [SerializeField] BossRoomManager bossRoomManager;
    private PlayerControl playerControl;
    private int blackBar = 50;  //测试用，实际应为0
    private int whiteBar = 50;  //测试用，实际应为0
    public int blackBarDisplay = 0;
    public int whiteBarDisplay = 0;
    public int blackBarMin = 0;
    public int whiteBarMin = 0;

    public Image blackBarImage;
    public Image whiteBarImage;
    private float maxWidth; // 黑白条最大宽度,即初始宽度

    private Vector3 respawnPoint;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        playerControl = player?.GetComponent<PlayerControl>();
        if (playerControl == null)
        {
            Debug.LogError("[LogicScript] Player 物体上未找到 PlayerControl 脚本！");
        }
        
        if (player != null)
        {
            respawnPoint = player.transform.position;
        }

        if (bossRoomManager == null)
        {
            bossRoomManager = FindObjectOfType<BossRoomManager>();
        }

        // 获取黑白条的最大宽度
        maxWidth = blackBarImage.rectTransform.rect.width;
    }

    public void SetRespawnPoint(Vector3 position)
    {
        respawnPoint = position;
        Debug.Log($"[LogicScript] Respawn point updated to {position}");
    }

    public void RespawnPlayer()
    {
        if (player != null)
        {
            player.transform.position = respawnPoint;
            // 初始化黑白条
            blackBar = 50; 
            whiteBar = 50;
            blackBarMin = 0;
            whiteBarMin = 0;
            Debug.Log("[LogicScript] Player respawned and stats reset.");

            if (bossRoomManager != null)
            {
                bossRoomManager.ResetEncounter();
            }
        }
    }

    private void UpdateBlackBarValue(float x)
    {
        float ratio = Mathf.Clamp01(x / 100f);
        blackBarImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth * ratio);
    }
    private void UpdateWhiteBarValue(float x)
    {
        float ratio = Mathf.Clamp01(x / 100f);
        whiteBarImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth * ratio);
    }

    public void BladeHitBlackEnemy()
    {
        if(playerControl.isBlack)
        {
            blackBar -= 2;
        }
    }

    public void BladeHitWhiteEnemy()
    {
        if(playerControl.isBlack)
        {
            whiteBar -= 1;
        }
    }

    public void LightBallHitBlackEnemy()
    {
        blackBar -= 1;
    }

    public void LightBallHitWhiteEnemy()
    {
        whiteBar -= 2;
    }

    public void HitByBlackEnemy()
    {
        if(playerControl.isBlack)
        {
            blackBar += 20;
        }
        else
        {
            blackBar += 10;
        }
    }
    public void HitByWhiteEnemy()
    {
        if(playerControl.isBlack)
        {
            whiteBar += 10;
        }
        else
        {
            whiteBar += 20;
        }
    }
    
    public void getIntoWhiteTrap()
    {
        whiteBarMin += 10;
    }
    public void getIntoBlackTrap()
    {
        blackBarMin += 10;
    }

    public void InBlackZone()
    {
        blackBar += 1;
        if(playerControl.isWhite)
        {
            whiteBar -= 1;
        }
    }
    public void InWhiteZone()
    {
        whiteBar += 1;
        if(playerControl.isBlack)
        {
            blackBar -= 1;
        }
    }

    public void FireLightBall()
    {
        whiteBar += 2;
    }

    public void AddBlackBar(int amount)
    {
        blackBar += amount;
    }

    public void AddWhiteBar(int amount)
    {
        whiteBar += amount;
    }

    private void Update()
    {   
        //场景黑白条交互
        /*
        timer += Time.deltaTime;
        if (timer >= 5f)    //每5秒触发一次场景黑白条交互
        {
            timer = 0f;
            InBlackZone();
            InWhiteZone();
        }
        */
        // 黑白条限制、死亡判断与UI显示
        if(blackBar < blackBarMin)
        {
            blackBar = blackBarMin;
        }
        if(whiteBar < whiteBarMin)
        {
            whiteBar = whiteBarMin;
        }
        if(blackBar > 100)
        {
            blackBar = 100;
        }
        if(whiteBar > 100)
        {
            whiteBar = 100;
        }

        blackBarDisplay = blackBar;
        whiteBarDisplay = whiteBar;
        UpdateBlackBarValue(blackBarDisplay);
        UpdateWhiteBarValue(whiteBarDisplay);

        if (blackBar == 0 || whiteBar == 0)
        {
            Debug.Log("获得成就：两袖清风（");
        }

        if (blackBar >= 100 || whiteBar >= 100)
        {
            Debug.Log("Game Over - Respawning...");
            RespawnPlayer();
        }
        
    }
    
}
