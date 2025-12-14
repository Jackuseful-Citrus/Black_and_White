using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Attach to the parent of platforms.
/// On enable: ensure each child platform has a TMP label child with a random insult.
/// Platforms also ping-pong vertically between lowerPoint and upperPoint.
/// </summary>
public class BlackMapStageOneJitter : MonoBehaviour
{
    [Header("Move Bounds")]
    [SerializeField] private Transform lowerPoint;
    [SerializeField] private Transform upperPoint;
    [SerializeField] private Vector2 moveSpeedRange = new Vector2(0.5f, 1.5f); // units per second
    [SerializeField] private bool useLocalBounds = true; // treat bounds as local to this parent

    [Header("TMP Label")]
    [SerializeField] private TMP_FontAsset labelFont;          // ✅ 可调字体
    [SerializeField] private float labelFontSize = 2.5f;       // ✅ 可调字号
    [SerializeField] private Color labelColor = Color.white;   // ✅ 可调颜色
    [SerializeField] private Vector3 labelLocalOffset = new Vector3(0f, 0.6f, 0f); // ✅ 文字相对平台偏移
    [SerializeField] private bool labelFaceCamera = false;     // 可选：让字面向相机（2D一般不需要）
    [SerializeField] private string labelChildName = "__JitterLabel";

    [Header("Insults")]
    [TextArea]
    [SerializeField]
    private string[] insults =
    {
        "You call that a jump?",
        "Try harder!",
        "Pathetic!",
        "Is that all?",
        "Weak sauce.",
        "Nice fall.",
        "Oops.",
        "Missed again!",
        "Come on!",
        "Too slow."
    };

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
        CacheChildrenAndEnsureLabels();
    }

    private void CacheChildrenAndEnsureLabels()
    {
        data.Clear();

        if (lowerPoint == null || upperPoint == null)
        {
            Debug.LogWarning($"{nameof(BlackMapStageOneJitter)} bounds not set.");
            return;
        }

        // Cache bounds once.
        float lower = GetBoundY(lowerPoint);
        float upper = GetBoundY(upperPoint);
        minY = Mathf.Min(lower, upper);
        maxY = Mathf.Max(lower, upper);

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null) continue;

            data.Add(new MoveData
            {
                t = child,
                basePos = child.localPosition,
                speed = Random.Range(moveSpeedRange.x, moveSpeedRange.y),
                phase = Random.Range(0f, 1f),
            });

            EnsureLabel(child);
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

            if (labelFaceCamera)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    Transform label = jd.t.Find(labelChildName);
                    if (label != null)
                    {
                        label.rotation = cam.transform.rotation;
                    }
                }
            }
        }
    }

    private float GetBoundY(Transform bound)
    {
        return useLocalBounds
            ? transform.InverseTransformPoint(bound.position).y
            : bound.position.y;
    }

    private void EnsureLabel(Transform platform)
    {
        if (platform == null) return;

        Transform existing = platform.Find(labelChildName);
        TextMeshPro tmp;

        if (existing == null)
        {
            GameObject go = new GameObject(labelChildName);
            go.transform.SetParent(platform, false);
            go.transform.localPosition = labelLocalOffset;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            tmp = go.AddComponent<TextMeshPro>();
        }
        else
        {
            tmp = existing.GetComponent<TextMeshPro>();
            if (tmp == null) tmp = existing.gameObject.AddComponent<TextMeshPro>();

            // 每次启用脚本也把偏移/样式校正一下
            existing.localPosition = labelLocalOffset;
            existing.localRotation = Quaternion.identity;
            existing.localScale = Vector3.one;
        }

        // 随机一句
        tmp.text = (insults != null && insults.Length > 0)
            ? insults[Random.Range(0, insults.Length)]
            : "Try harder!";

        // 样式（可调）
        if (labelFont != null) tmp.font = labelFont;
        tmp.fontSize = labelFontSize;
        tmp.color = labelColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;

        // 让它别吃到奇怪的Z深度（2D里常用）
        var p = tmp.transform.localPosition;
        tmp.transform.localPosition = new Vector3(p.x, p.y, -2f);
        Vector3 ps = platform.lossyScale;
        if (Mathf.Abs(ps.x) < 1e-6f) ps.x = 1f;
        if (Mathf.Abs(ps.y) < 1e-6f) ps.y = 1f;

        // 注意：lossyScale 是世界缩放，所以这里用一个近似做法：让 label 在世界里接近 1,1,1
        tmp.transform.localScale = new Vector3(
            1f / platform.localScale.x,
            1f / platform.localScale.y
        );
    }
}
