using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogicScript : MonoBehaviour
{
    public static LogicScript Instance;

    [SerializeField] GameObject player;
    [SerializeField] BossRoomManager bossRoomManager;
    private PlayerControl playerControl;
    private int blackBar = 70;  //测试用，实际应为0
    private int whiteBar = 50;  //测试用，实际应为0
    public int blackBarDisplay = 0;
    public int whiteBarDisplay = 0;
    public int blackBarMin = 0;
    public int whiteBarMin = 0;

    public Image blackBarImage;
    public Image whiteBarImage;
    private float maxWidth; // 黑白条最大宽度,即初始宽度

    private Vector3 respawnPoint;

    private static bool hasPendingTeleport = false;
    private static Vector3 pendingTeleportPoint = Vector3.zero;
    private static bool hasPendingDoorTeleport = false;

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
        // 如果没有手动指定 player，就自动查找
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
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

        maxWidth = blackBarImage.rectTransform.rect.width;
        
        if (hasPendingDoorTeleport)
        {
            hasPendingDoorTeleport = false;
            Door door = FindObjectOfType<Door>();
            if (door != null)
            {
                Vector3 spawnPoint = door.GetSpawnPoint();
                respawnPoint = spawnPoint;

                if (player != null)
                {
                    player.transform.position = spawnPoint;
                }
                Debug.Log($"[LogicScript] 通过门传送，出生在新场景门口：{spawnPoint}");
            }
            else
            {
                Debug.LogWarning("[LogicScript] 通过门传送，但新场景中未找到 Door 组件。");
            }
        }
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
    public int GetBlackBar()
    {
        return blackBar;
    }

    public int GetWhiteBar()
    {
        return whiteBar;
    }

    public void SetTeleportPoint(Vector3 point)
    {
        respawnPoint = point;
        Debug.Log($"[LogicScript] 传送点已设置为：{point}");
    }

    public Vector3 GetTeleportPoint()
    {
        return respawnPoint;
    }

    public void TeleportPlayerToScene(Vector3 teleportPos, string sceneName)
    {
        pendingTeleportPoint = teleportPos;
        hasPendingTeleport = true;
        SceneManager.LoadScene(sceneName);
    }

    public void TeleportViaDoor(string sceneName)
    {
        // 标记一下：下一次进入新场景时，要用“门”的位置作为出生点
        hasPendingDoorTeleport = true;

        // 不设置具体坐标，等新场景 Start 里用 Door 的位置
        SceneManager.LoadScene(sceneName);
    }


    // 供玩家读取
    public Vector3 ConsumePendingTeleportPoint()
    {
        if (!hasPendingTeleport) return Vector3.zero;
        hasPendingTeleport = false;
        respawnPoint = pendingTeleportPoint;
        return pendingTeleportPoint;
    }
}
