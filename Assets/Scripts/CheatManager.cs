using UnityEngine;
using System.Collections.Generic;

public class CheatManager : MonoBehaviour
{
    [Header("Teleport Locations")]
    public Vector3 location1;
    public Vector3 location2;
    public Vector3 location3;
    public Vector3 location4;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TeleportTo(location1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TeleportTo(location2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TeleportTo(location3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TeleportTo(location4);
        }
    }

    private void TeleportTo(Vector3 targetPos)
    {
        if (LogicScript.Instance != null)
        {
            Debug.Log($"[CheatManager] Teleporting to {targetPos}");
            LogicScript.Instance.SetRespawnPoint(targetPos);
            LogicScript.Instance.RespawnPlayer();
        }
        else
        {
            Debug.LogError("[CheatManager] LogicScript Instance not found!");
        }
    }
}
