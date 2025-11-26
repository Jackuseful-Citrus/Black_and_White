using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeScript : MonoBehaviour
{
    private CapsuleCollider2D capsuleCollider2D;
    private void Start()
    {
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        capsuleCollider2D.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WhiteEnemy"))
        {            
            if (LogicScript.Instance != null)
            {
                LogicScript.Instance.BladeHitWhiteEnemy();
            }            
        }
        else if (collision.gameObject.CompareTag("BlackEnemy"))
        {
            if (LogicScript.Instance != null)
            {
                LogicScript.Instance.BladeHitBlackEnemy();
            }
        }
    }
    
}
