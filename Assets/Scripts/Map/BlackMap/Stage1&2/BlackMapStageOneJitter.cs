using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves Stage1 platforms between two vertical bounds instead of jittering in place.
/// Attach to the parent; each child will ping-pong between lowerPoint and upperPoint.
/// </summary>
public class BlackMapStageOneJitter : MonoBehaviour
{
    [SerializeField] private Transform lowerPoint;
    [SerializeField] private Transform upperPoint;
    [SerializeField] private Vector2 moveSpeedRange = new Vector2(0.5f, 1.5f); // units per second
    [SerializeField] private bool useLocalBounds = true; // treat bounds as local to this parent

    private struct MoveData
    {
        public Transform t;
        public float speed;
        public float phase;
        public Vector3 basePos;
    }

    private readonly List<MoveData> data = new List<MoveData>();
    private float minY;
    private float maxY;

    private void OnEnable()
    {
        CacheChildren();
    }

    private void CacheChildren()
    {
        if (lowerPoint == null || upperPoint == null)
        {
            data.Clear();
            Debug.LogWarning($"{nameof(BlackMapStageOneJitter)} bounds not set.");
            return;
        }

        // Cache bounds once to avoid recomputing every frame.
        float lower = GetBoundY(lowerPoint);
        float upper = GetBoundY(upperPoint);
        minY = Mathf.Min(lower, upper);
        maxY = Mathf.Max(lower, upper);

        data.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null) continue;

            MoveData jd = new MoveData
            {
                t = child,
                basePos = child.localPosition,
                speed = Random.Range(moveSpeedRange.x, moveSpeedRange.y),
                phase = Random.Range(0f, 1f) // start at a different point in the cycle
            };
            data.Add(jd);
        }
    }

    private void Update()
    {
        float time = Time.time;
        for (int i = 0; i < data.Count; i++)
        {
            MoveData jd = data[i];
            if (jd.t == null) continue;

            float normalized = Mathf.PingPong((time + jd.phase) * jd.speed, 1f);
            float y = Mathf.Lerp(minY, maxY, normalized);
            jd.t.localPosition = new Vector3(jd.basePos.x, y, jd.basePos.z);
        }
    }

    private float GetBoundY(Transform bound)
    {
        return useLocalBounds
            ? transform.InverseTransformPoint(bound.position).y
            : bound.position.y;
    }
}
