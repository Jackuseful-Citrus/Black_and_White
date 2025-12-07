using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class VortexFlowBinder : MonoBehaviour
{
    public Transform center;
    [Range(0f, 3f)] public float speed = 0.4f;
    [Range(0f, 1f)] public float flowScale = 0.08f;

    static readonly int CenterId = Shader.PropertyToID("_Center");
    static readonly int SpeedId = Shader.PropertyToID("_Speed");
    static readonly int FlowScaleId = Shader.PropertyToID("_FlowScale");

    private Renderer rend;
    private MaterialPropertyBlock mpb;
    static readonly int HalfSizeId = Shader.PropertyToID("_HalfSize");
    public Vector2 halfSize = new Vector2(6, 4);


    void OnEnable()
    {
        rend = GetComponent<Renderer>();
        mpb ??= new MaterialPropertyBlock();
    }

    void LateUpdate()
    {
        if (!rend) return;

        rend.GetPropertyBlock(mpb);

        Vector3 c = center ? center.position : Vector3.zero;
        mpb.SetVector(CenterId, new Vector4(c.x, c.y, 0, 0));
        mpb.SetFloat(SpeedId, speed);
        mpb.SetFloat(FlowScaleId, flowScale);
        mpb.SetVector(HalfSizeId, halfSize);
        rend.SetPropertyBlock(mpb);
    }
}
