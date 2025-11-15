using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBallSpawnerScript : MonoBehaviour
{
    [SerializeField] GameObject LightBall;
    [SerializeField] float spawnRate = 2;
    private float timer = 0;
    [SerializeField] float widthOffset = 2;

    private void Start()
    {
        SpawnLightBall();
    }

    void Update()
    {
        if (timer < spawnRate)
        {
            timer = timer + Time.deltaTime;
        }
        else
        {
            SpawnLightBall();
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
