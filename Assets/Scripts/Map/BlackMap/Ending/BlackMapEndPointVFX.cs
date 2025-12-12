using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controls endpoint visual: slow rotation + vertical bob + pulsating light.
/// Attach to EndPoint root; assign light2D and optional target transform to rotate/bob.
/// </summary>
public class BlackMapEndPointVFX : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private Transform target;
    [SerializeField] private float rotationSpeedDeg = 20f;

    [Header("Bob")]
    [SerializeField] private float bobAmplitude = 0.2f;
    [SerializeField] private float bobFrequency = 0.5f; // Hz

    [Header("Light Pulse")]
    [SerializeField] private Light2D light2D;
    [SerializeField] private float lightIntensityMin = 0.6f;
    [SerializeField] private float lightIntensityMax = 1.2f;
    [SerializeField] private float lightPulseFrequency = 0.6f; // Hz
    [SerializeField] private float lightRadiusMin = 1.5f;
    [SerializeField] private float lightRadiusMax = 2.5f;

    private Vector3 basePos;

    private void Awake()
    {
        if (target == null) target = transform;
        basePos = target.localPosition;
    }

    private void Update()
    {
        float time = Time.time;

        // rotation
        if (rotationSpeedDeg != 0f && target != null)
        {
            target.Rotate(0f, 0f, rotationSpeedDeg * Time.deltaTime, Space.Self);
        }

        // bob
        if (target != null && bobAmplitude > 0f && bobFrequency > 0f)
        {
            float bob = Mathf.Sin(time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
            target.localPosition = basePos + new Vector3(0f, bob, 0f);
        }

        // light pulse
        if (light2D != null)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(time * lightPulseFrequency * Mathf.PI * 2f);
            float intensity = Mathf.Lerp(lightIntensityMin, lightIntensityMax, pulse);
            float radius = Mathf.Lerp(lightRadiusMin, lightRadiusMax, pulse);
            light2D.intensity = intensity;
            light2D.pointLightOuterRadius = radius;
        }
    }
}
