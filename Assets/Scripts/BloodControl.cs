using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodControl : MonoBehaviour
{
    public float minValue = 0f;
    public float maxValue = 100f;

    public float whiteBlood = 50f;
    public float blackBlood = 50f;

    public void AddWhiteMinusBlack(float v)
    {
        whiteBlood = Mathf.Clamp(whiteBlood + v, minValue, maxValue);
        blackBlood = Mathf.Clamp(blackBlood - v, minValue, maxValue);
    }

    public void AddBlackMinusWhite(float v)
    {
        blackBlood = Mathf.Clamp(blackBlood + v, minValue, maxValue);
        whiteBlood = Mathf.Clamp(whiteBlood - v, minValue, maxValue);
    }
}
