using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Move multiple platforms along a tilted capsule (rounded-rectangle) path, like an escalator.
/// Each platform also has a small horizontal sway.
/// </summary>
public class BlackMapEscalator : MonoBehaviour
{
    [Header("Platform Spawn")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private int platformCount = 10;
    [SerializeField] private bool spawnOnStart = true;

    [Header("Capsule Path")]
    [SerializeField] private float capsuleLength = 8f;   // Straight segment length between arc centers
    [SerializeField] private float capsuleRadius = 2f;   // Arc radius (thickness)
    [SerializeField] private float tiltAngleDeg = 25f;   // Rotation of the whole capsule
    [SerializeField] private float angularSpeedDeg = 90f; // Parametric speed along the path (deg/sec)

    [Header("Horizontal Sway")]
    [SerializeField] private Vector2 swayAmplitudeRange = new Vector2(0.15f, 0.35f); // world X offset amplitude
    [SerializeField] private Vector2 swaySpeedRange = new Vector2(0.8f, 1.4f);      // oscillation frequency in Hz

    [Header("Lifecycle")]
    [SerializeField] private bool destroyOnDisable = true;

    private readonly List<Transform> spawnedPlatforms = new List<Transform>();
    private readonly List<float> swayAmplitudes = new List<float>();
    private readonly List<float> swaySpeeds = new List<float>();
    private readonly List<float> swayPhases = new List<float>();
    private float phaseStepDeg;

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnPlatforms();
        }
    }

    private void Update()
    {
        if (spawnedPlatforms.Count == 0) return;

        float timeAngle = Time.time * angularSpeedDeg;
        float tiltRad = tiltAngleDeg * Mathf.Deg2Rad;
        float cosTilt = Mathf.Cos(tiltRad);
        float sinTilt = Mathf.Sin(tiltRad);
        float len = Mathf.Max(0.01f, capsuleLength);
        float r = Mathf.Max(0.01f, capsuleRadius);

        for (int i = 0; i < spawnedPlatforms.Count; i++)
        {
            Transform plat = spawnedPlatforms[i];
            if (plat == null) continue;

            float phase = (timeAngle + i * phaseStepDeg) / 360f; // 0~1 loop
            Vector2 capsulePos = EvaluateCapsule(phase, len, r);

            float rotatedX = capsulePos.x * cosTilt - capsulePos.y * sinTilt;
            float rotatedY = capsulePos.x * sinTilt + capsulePos.y * cosTilt;
            Vector3 worldPos = transform.position + new Vector3(rotatedX, rotatedY, 0f);

            // Horizontal random sway in world X
            float amp = (i < swayAmplitudes.Count) ? swayAmplitudes[i] : 0f;
            float spd = (i < swaySpeeds.Count) ? swaySpeeds[i] : 0f;
            float phs = (i < swayPhases.Count) ? swayPhases[i] : 0f;
            if (amp > 0.0001f && spd > 0.0001f)
            {
                float sway = Mathf.Sin(Time.time * spd * Mathf.PI * 2f + phs) * amp;
                worldPos += Vector3.right * sway;
            }

            plat.position = worldPos;
        }
    }

    public void SpawnPlatforms()
    {
        CleanupDestroyed();

        if (platformPrefab == null || platformCount <= 0) return;
        phaseStepDeg = 360f / Mathf.Max(1, platformCount);

        for (int i = 0; i < platformCount; i++)
        {
            GameObject plat = Instantiate(platformPrefab, transform.position, Quaternion.identity, transform);
            spawnedPlatforms.Add(plat.transform);

            float amp = Random.Range(swayAmplitudeRange.x, swayAmplitudeRange.y);
            float spd = Random.Range(swaySpeedRange.x, swaySpeedRange.y);
            float phs = Random.Range(0f, Mathf.PI * 2f);
            swayAmplitudes.Add(Mathf.Max(0f, amp));
            swaySpeeds.Add(Mathf.Max(0f, spd));
            swayPhases.Add(phs);
        }
    }

    public void ClearPlatforms()
    {
        for (int i = 0; i < spawnedPlatforms.Count; i++)
        {
            Transform plat = spawnedPlatforms[i];
            if (plat != null)
            {
                if (destroyOnDisable)
                {
                    Destroy(plat.gameObject);
                }
                else
                {
                    plat.gameObject.SetActive(false);
                }
            }
        }
        spawnedPlatforms.Clear();
        swayAmplitudes.Clear();
        swaySpeeds.Clear();
        swayPhases.Clear();
    }

    private void OnDisable()
    {
        if (destroyOnDisable)
        {
            ClearPlatforms();
        }
    }

    private void CleanupDestroyed()
    {
        for (int i = spawnedPlatforms.Count - 1; i >= 0; i--)
        {
            if (spawnedPlatforms[i] == null)
            {
                spawnedPlatforms.RemoveAt(i);
                if (i < swayAmplitudes.Count) swayAmplitudes.RemoveAt(i);
                if (i < swaySpeeds.Count) swaySpeeds.RemoveAt(i);
                if (i < swayPhases.Count) swayPhases.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position;

        float len = Mathf.Max(0.01f, capsuleLength);
        float r = Mathf.Max(0.01f, capsuleRadius);
        float tiltRad = tiltAngleDeg * Mathf.Deg2Rad;
        float cosTilt = Mathf.Cos(tiltRad);
        float sinTilt = Mathf.Sin(tiltRad);

        const int segments = 80;
        Vector3 prev = center;
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector2 capsulePos = EvaluateCapsule(t, len, r);
            float rx = capsulePos.x * cosTilt - capsulePos.y * sinTilt;
            float ry = capsulePos.x * sinTilt + capsulePos.y * cosTilt;
            Vector3 p = center + new Vector3(rx, ry, 0f);
            if (i > 0)
            {
                Gizmos.DrawLine(prev, p);
            }
            prev = p;
        }
    }

    /// <summary>
    /// Map phase [0,1) to a capsule path: top line -> right half-circle -> bottom line -> left half-circle.
    /// </summary>
    private Vector2 EvaluateCapsule(float phase, float length, float radius)
    {
        float straight = Mathf.Max(0.001f, length);
        float arcLen = Mathf.PI * radius; // half-circle length
        float total = straight * 2f + arcLen * 2f;

        float s = Mathf.Repeat(phase, 1f) * total;
        float halfLen = straight * 0.5f;

        // Top straight: left -> right at y = +r
        if (s < straight)
        {
            float t = s / straight;
            float x = Mathf.Lerp(-halfLen, halfLen, t);
            return new Vector2(x, radius);
        }
        s -= straight;

        // Right arc: 90deg -> -90deg, center at (+halfLen, 0)
        if (s < arcLen)
        {
            float t = s / arcLen;
            float ang = Mathf.Lerp(90f, -90f, t) * Mathf.Deg2Rad;
            float x = halfLen + Mathf.Cos(ang) * radius;
            float y = Mathf.Sin(ang) * radius;
            return new Vector2(x, y);
        }
        s -= arcLen;

        // Bottom straight: right -> left at y = -r
        if (s < straight)
        {
            float t = s / straight;
            float x = Mathf.Lerp(halfLen, -halfLen, t);
            return new Vector2(x, -radius);
        }
        s -= straight;

        // Left arc: 270deg -> 90deg (bottom -> top), center at (-halfLen, 0)
        {
            float t = s / arcLen;
            float ang = Mathf.Lerp(270f, 90f, t) * Mathf.Deg2Rad;
            float x = -halfLen + Mathf.Cos(ang) * radius;
            float y = Mathf.Sin(ang) * radius;
            return new Vector2(x, y);
        }
    }
}
