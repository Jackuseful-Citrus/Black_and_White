using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicScript : MonoBehaviour
{
    [SerializeField] GameObject player;
    private PlayerControl playerControl;
    private int blackBar = 0;
    private int whiteBar = 0;
    public int blackBarDisplay = 0;
    public int whiteBarDisplay = 0;
    public int blackBarMin = 0;
    public int whiteBarMin = 0;
    private float timer = 0f;

    private void Start()
    {
        playerControl = player?.GetComponent<PlayerControl>();
        if (playerControl == null)
        {
            Debug.LogError("[LogicScript] Player 物体上未找到 PlayerControl 脚本！");
        }
        
    }
    private void HitBlackEnemy()
    {
        if(playerControl.isBlack)
        {
            blackBar -= 2;
        }
        else
        {
            blackBar -= 1;
        }
    }

    private void HitWhiteEnemy()
    {
        if(playerControl.isBlack)
        {
            whiteBar -= 1;
        }
        else
        {
            whiteBar -= 2;
        }
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
    
    private void getIntoWhiteTrap()
    {
        whiteBarMin += 10;
    }
    private void getIntoBlackTrap()
    {
        blackBarMin += 10;
    }

    private void InBlackZone()
    {
        blackBar += 1;
        if(playerControl.isWhite)
        {
            whiteBar -= 1;
        }
    }
    private void InWhiteZone()
    {
        whiteBar += 1;
        if(playerControl.isBlack)
        {
            blackBar -= 1;
        }
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
        if (blackBar >= 100 || whiteBar >= 100)
        {
            Debug.Log("Game Over");
        }
        
    }
    
}
