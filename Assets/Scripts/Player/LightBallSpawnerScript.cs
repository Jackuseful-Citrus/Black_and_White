using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LightBallSpawnerScript : MonoBehaviour
{
    [SerializeField] GameObject LightBall;
    [SerializeField] GameObject Player;

    private float timer = 0;
    [SerializeField] float widthOffset = 0.3f; //生成光球的范围偏移量

    private PlayerControl playerControl;
    public bool inAttackRecovery = false;

    private void Start()
    {
        playerControl = Player?.GetComponent<PlayerControl>();
        if (playerControl == null)
        {
            Debug.LogError("[ScytheScript] Player 物体上未找到 PlayerControl 脚本！");
        }
    }

    void Update()
    {
        if (playerControl.isAttacking)  //长按开始施法，每隔0.5秒生成一个光球,0.5秒的施法后摇
        {
            if (timer < 0.5f && timer > 0f)
            {
                inAttackRecovery = false;
                timer = timer + Time.deltaTime;
            }
            else if (timer >= 0.5f)
            {
                SpawnLightBall();
                timer = -0.5f;
                inAttackRecovery = true;
            }
            else if (timer <= 0f)
            {
                timer = timer + Time.deltaTime;
            }
        }
        
    }
    void SpawnLightBall()
    {
        float leftestPoint = transform.position.x - widthOffset;
        float rightestPoint = transform.position.x + widthOffset;
        float lowerPoint = transform.position.y - widthOffset;
        float higherPoint = transform.position.y + widthOffset;
        Instantiate(LightBall, new Vector3(Random.Range(leftestPoint,rightestPoint)
            ,Random.Range(lowerPoint,higherPoint),0.003f), transform.rotation);
            timer = 0;
    }
}
