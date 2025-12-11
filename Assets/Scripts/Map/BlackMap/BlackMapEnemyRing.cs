using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 生成一圈敌人并让整圈沿指定角度进行往复（30 度默认）运动，同时环自身可自转。
/// 可调参数在 Inspector 内配置。
/// </summary>
public class BlackMapEnemyRing : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 10;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool destroyOnDisable = true;

    [Header("Ring Shape")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float initialRotationDeg = 0f; // 整圈初始朝向

    [Header("Ring Rotation")]
    [SerializeField] private float ringAngularSpeedDeg = 60f; // 度/秒，自转速度

    [Header("Travel (Ping-Pong)")]
    [SerializeField] private float travelAngleDeg = 30f;   // 沿该角度方向移动（相对世界 X 轴）
    [SerializeField] private float travelAmplitude = 4f;   // 往返振幅（世界坐标）
    [SerializeField] private float travelFrequency = 0.25f; // 往返频率（Hz）

    private readonly List<Transform> spawned = new List<Transform>();
    private float phaseStepDeg;

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnRing();
        }
    }

    private void Update()
    {
        if (spawned.Count == 0) return;

        float time = Time.time;

        // 往复位移：沿 travelAngleDeg 方向的正弦平移
        float travelPhase = Mathf.Sin(time * travelFrequency * Mathf.PI * 2f);
        float travelDistance = travelPhase * travelAmplitude;
        float travelRad = travelAngleDeg * Mathf.Deg2Rad;
        Vector3 travelDir = new Vector3(Mathf.Cos(travelRad), Mathf.Sin(travelRad), 0f);
        Vector3 travelOffset = travelDir * travelDistance;

        // 环自转角度
        float ringAngle = (initialRotationDeg + time * ringAngularSpeedDeg) * Mathf.Deg2Rad;
        float cosA = Mathf.Cos(ringAngle);
        float sinA = Mathf.Sin(ringAngle);

        float r = Mathf.Max(0.01f, radius);

        for (int i = 0; i < spawned.Count; i++)
        {
            Transform t = spawned[i];
            if (t == null) continue;

            float enemyAngle = (i * phaseStepDeg) * Mathf.Deg2Rad;
            float xLocal = Mathf.Cos(enemyAngle) * r;
            float yLocal = Mathf.Sin(enemyAngle) * r;

            // 应用整圈自转
            float xRot = xLocal * cosA - yLocal * sinA;
            float yRot = xLocal * sinA + yLocal * cosA;

            Vector3 pos = transform.position + travelOffset + new Vector3(xRot, yRot, 0f);
            t.position = pos;
        }
    }

    public void SpawnRing()
    {
        ClearRing();

        if (enemyPrefab == null || enemyCount <= 0) return;
        phaseStepDeg = 360f / Mathf.Max(1, enemyCount);

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject go = Instantiate(enemyPrefab, transform.position, Quaternion.identity, transform);
            spawned.Add(go.transform);
        }
    }

    public void ClearRing()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            Transform t = spawned[i];
            if (t != null)
            {
                if (destroyOnDisable)
                {
                    Destroy(t.gameObject);
                }
                else
                {
                    t.gameObject.SetActive(false);
                }
            }
        }
        spawned.Clear();
    }

    private void OnDisable()
    {
        if (destroyOnDisable)
        {
            ClearRing();
        }
    }
}
