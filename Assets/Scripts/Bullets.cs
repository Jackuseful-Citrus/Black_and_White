using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullets : MonoBehaviour
{
    public enum BulletType { White, Black }
    public BulletType bulletType = BulletType.White;
    public float damage = 10f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Bullet hit: " + collision.gameObject.name);
        Playercontrol player = collision.gameObject.GetComponent<Playercontrol>();
        BloodControl blood = collision.gameObject.GetComponent<BloodControl>();

        if (player != null && blood != null)
        {
            Debug.Log("Bullet in" );
            if (player.isWhite && bulletType == BulletType.White)
            {
                blood.AddWhiteMinusBlack(damage);
                Debug.Log($"White hit -> white:{blood.whiteBlood} black:{blood.blackBlood}");
            }
            else if (player.isBlack && bulletType == BulletType.Black)
            {
                blood.AddBlackMinusWhite(damage);
                Debug.Log($"Black hit -> black:{blood.blackBlood} white:{blood.whiteBlood}");
            }
        }
        Destroy(gameObject);
    }
}
