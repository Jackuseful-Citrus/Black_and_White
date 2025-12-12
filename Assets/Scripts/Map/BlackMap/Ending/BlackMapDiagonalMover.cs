using UnityEngine;

/// <summary>
/// Mover with optional oscillation: linear motion plus sine wobble.
/// </summary>
public class BlackMapDiagonalMover : MonoBehaviour
{
    [SerializeField] private Vector2 direction = new Vector2(-1f, -1f);
    [SerializeField] private float speed = 3f;
    [SerializeField] private float lifeTime = 12f;

    [Header("Oscillation")]
    [SerializeField] private float oscAmplitude = 0f;
    [SerializeField] private float oscFrequency = 1f; // Hz
    [SerializeField] private Vector2 oscDirection = new Vector2(0f, 1f);
    [SerializeField] private float oscPhase = 0f;

    private float endTime = float.PositiveInfinity;
    private Vector3 startPos;
    private float startTime;
    private bool initialized;

    public void Init(Vector2 dir, float moveSpeed, float life)
    {
        Init(dir, moveSpeed, life, 0f, 1f, Vector2.up, 0f);
    }

    public void Init(Vector2 dir, float moveSpeed, float life, float wobbleAmp, float wobbleFreq, Vector2 wobbleDir, float wobblePhase)
    {
        direction = dir;
        speed = moveSpeed;
        lifeTime = life;
        oscAmplitude = Mathf.Max(0f, wobbleAmp);
        oscFrequency = Mathf.Max(0f, wobbleFreq);
        oscDirection = wobbleDir;
        oscPhase = wobblePhase;

        startPos = transform.position;
        startTime = Time.time;
        endTime = lifeTime > 0f ? startTime + lifeTime : float.PositiveInfinity;
        initialized = true;
    }

    private void OnEnable()
    {
        if (!initialized)
        {
            startPos = transform.position;
            startTime = Time.time;
            endTime = lifeTime > 0f ? startTime + lifeTime : float.PositiveInfinity;
        }
    }

    private void Update()
    {
        float elapsed = Time.time - startTime;
        Vector2 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.zero;
        Vector3 linear = startPos + (Vector3)(dir * speed * elapsed);

        Vector3 wobble = Vector3.zero;
        if (oscAmplitude > 0.0001f && oscFrequency > 0.0001f)
        {
            Vector2 wobbleDir = oscDirection.sqrMagnitude > 0.0001f ? oscDirection.normalized : Vector2.up;
            float wobbleOffset = Mathf.Sin((elapsed * oscFrequency * Mathf.PI * 2f) + oscPhase) * oscAmplitude;
            wobble = (Vector3)(wobbleDir * wobbleOffset);
        }

        transform.position = linear + wobble;

        if (Time.time >= endTime)
        {
            Destroy(gameObject);
        }
    }
}
