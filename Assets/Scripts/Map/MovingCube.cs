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
        if (timer < 4f)
        {
            transform.position += new Vector3(2f, 0f, 0f) * Time.deltaTime;
        }
        if (timer >= 4f && timer < 8f)
        {
            transform.position += new Vector3(-2f, 0f, 0f) * Time.deltaTime;
        }
        if (timer >= 8f)
        {
            timer = 0f;
        }
    }
}
