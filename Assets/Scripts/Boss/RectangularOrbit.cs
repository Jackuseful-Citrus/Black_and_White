using UnityEngine;

public class RectangularOrbit : MonoBehaviour
{
    [Header("Orbit Target")]
    public Transform center;

    [Header("Half Extents (from center)")]
    public float halfWidth = 6f;   // 房间中心到左右边缘的半宽
    public float halfHeight = 4f;  // 房间中心到上下边缘的半高

    [Header("Motion")]
    public float speed = 2f;       // 沿边走的速度（世界单位/秒）
    public bool clockwise = true;

    [Header("Optional Wobble")]
    public float outwardWobble = 0.0f; // 向外轻微呼吸
    public float wobbleFreq = 1.5f;

    private float dist; // 当前沿边走过的距离

    void Update()
    {
        if (!center) return;

        float w = Mathf.Max(0.01f, halfWidth);
        float h = Mathf.Max(0.01f, halfHeight);

        float perim = 4f * (w + h);

        float dir = clockwise ? 1f : -1f;
        dist = (dist + dir * speed * Time.deltaTime) % perim;
        if (dist < 0) dist += perim;

        Vector2 local = DistanceToRectPoint(dist, w, h);

        // 轻微“结界呼吸感”
        if (outwardWobble > 0.0001f)
        {
            float wob = Mathf.Sin(Time.time * wobbleFreq) * outwardWobble;
            Vector2 normal = RectNormalAt(local, w, h);
            local += normal * wob;
        }

        transform.position = center.position + new Vector3(local.x, local.y, 0f);
    }

    // 按顺时针从“右上角向左”开始走一圈
    private Vector2 DistanceToRectPoint(float d, float w, float h)
    {
        // 边长
        float top = 2f * w;
        float right = 2f * h;
        float bottom = 2f * w;
        float left = 2f * h;

        // 右上角
        float x = w;
        float y = h;

        if (d < top)
        {
            // 顶边：从 (w, h) -> (-w, h)
            x = w - d;
            y = h;
        }
        else if ((d -= top) < left)
        {
            // 左边：从 (-w, h) -> (-w, -h)
            x = -w;
            y = h - d;
        }
        else if ((d -= left) < bottom)
        {
            // 底边：从 (-w, -h) -> (w, -h)
            x = -w + d;
            y = -h;
        }
        else
        {
            // 右边：从 (w, -h) -> (w, h)
            d -= bottom;
            x = w;
            y = -h + d;
        }

        return new Vector2(x, y);
    }

    // 给“向外呼吸”用的法线方向
    private Vector2 RectNormalAt(Vector2 p, float w, float h)
    {
        // 判断离哪条边更近
        float dx = Mathf.Abs(Mathf.Abs(p.x) - w);
        float dy = Mathf.Abs(Mathf.Abs(p.y) - h);

        if (dx < dy)
        {
            // 更靠左右边
            return new Vector2(Mathf.Sign(p.x), 0f);
        }
        else
        {
            // 更靠上下边
            return new Vector2(0f, Mathf.Sign(p.y));
        }
    }
}
