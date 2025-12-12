using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adds small random jitter (Brownian-like) to Stage1 platforms.
/// Attach this to a parent with child platforms; it will add per-platform offsets.
/// </summary>
public class BlackMapStageOneJitter : MonoBehaviour
{
    [SerializeField] private Vector2 jitterAmpRange = new Vector2(0.05f, 0.12f); // vertical amplitude range
    [SerializeField] private Vector2 jitterFreqRange = new Vector2(0.6f, 1.4f); // Hz, vertical oscillation

    private struct JitterData
    {
        public Transform t;
        public float amp;
        public float freq;
        public float phase;
        public Vector3 basePos;
    }

    private readonly List<JitterData> data = new List<JitterData>();

    private void OnEnable()
    {
        CacheChildren();
    }

    private void CacheChildren()
    {
        data.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null) continue;

            JitterData jd = new JitterData
            {
                t = child,
                basePos = child.localPosition,
                amp = Random.Range(jitterAmpRange.x, jitterAmpRange.y),
                freq = Random.Range(jitterFreqRange.x, jitterFreqRange.y),
                phase = Random.Range(0f, Mathf.PI * 2f)
            };
            data.Add(jd);
        }
    }

    private void Update()
    {
        float time = Time.time;
        for (int i = 0; i < data.Count; i++)
        {
            JitterData jd = data[i];
            if (jd.t == null) continue;

            float oscY = Mathf.Sin(time * jd.freq * Mathf.PI * 2f + jd.phase) * jd.amp;
            jd.t.localPosition = jd.basePos + new Vector3(0f, oscY, 0f);
        }
    }
}
