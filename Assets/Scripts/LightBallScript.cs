using UnityEngine;

public class LightBallScript : MonoBehaviour
{
    [SerializeField] private float acceleration = 3f;
    private Vector3 launchDir = Vector3.zero;
    private float maxSpeed = 5f;
    private float speed = 0f;
    private float timer = 0f;
    private Vector3 moveDir = Vector3.zero;


    private void Awake()
    {
        var actions = InputManager.Instance.PlayerInputActions;
        actions.Player.MousePosition.performed += ctx =>
        {
            Vector2 mouseScreenPos = ctx.ReadValue<Vector2>();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f)
            );
            launchDir = (worldPos - transform.position).normalized;
        };
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < 2f)
        {
            speed += acceleration * Time.deltaTime;
            speed = Mathf.Min(speed, maxSpeed);
            moveDir = launchDir;
        }
        transform.position += moveDir * speed * Time.deltaTime;
        if (timer >= 5f)
        {
            gameObject.SetActive(false);
        }
        
    }
}
