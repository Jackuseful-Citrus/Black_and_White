using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCubeHorizontal : MonoBehaviour
{
    private float timer = 0f;
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < 2.5f)
        {
            transform.position += new Vector3(10f, 0f, 0f) * Time.deltaTime;
        }
        if (timer >= 2.5f && timer < 5f)
        {
            transform.position += new Vector3(-10f, 0f, 0f) * Time.deltaTime;
        }
        if (timer >= 5f)
        {
            timer = 0f;
        }
    }
}
