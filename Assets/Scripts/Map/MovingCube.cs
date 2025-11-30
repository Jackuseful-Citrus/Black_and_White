using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private float timer = 0f;
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < 3f)
        {
            transform.position += new Vector3(0f, 2f, 0f) * Time.deltaTime;
        }
        if (timer >= 3f && timer < 6f)
        {
            transform.position += new Vector3(0f, -2f, 0f) * Time.deltaTime;
        }
        if (timer >= 6f)
        {
            timer = 0f;
        }
    }
}
