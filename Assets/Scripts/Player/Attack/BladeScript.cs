using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeScript : MonoBehaviour
{
    private CapsuleCollider2D capsuleCollider2D;
    [SerializeField] private LogicScript logicScript;
    private void Start()
    {
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        capsuleCollider2D.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WhiteEnemy"))
        {            
            if (logicScript != null)
            {
                logicScript.HitWhiteEnemy();
            }            
        }
        else if (collision.gameObject.CompareTag("BlackEnemy"))
        {
            if (logicScript != null)
            {
                logicScript.HitBlackEnemy();
                Debug.Log("Hit Black Enemy");
            }
        }
    }
    
}
