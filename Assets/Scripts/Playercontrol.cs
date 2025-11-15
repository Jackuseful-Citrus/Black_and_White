using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControl : MonoBehaviour
{
    [SerializeField] float speed = 6f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float jumpCutMultiplier = 0.1f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundRadius = 0.1f;
    [SerializeField] LayerMask groundLayer;

    Rigidbody2D rb;
    float horiz = 0f;
    bool isGrounded;

    public GameObject BlackOutlook;
    public GameObject WhiteOutlook;
    private bool isWhiteOutlook = false;
    private bool isSwitching = false;

    public bool isWhite => isWhiteOutlook;
    public bool isBlack => !isWhiteOutlook;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        BlackOutlook.SetActive(true);
        WhiteOutlook.SetActive(false);
    }

    private void Awake()
    {
        // 使用全局 InputManager 实例
        var actions = InputManager.Instance.PlayerInputActions;

        actions.Player.Jump.performed += ctx =>
        {
            if (isSwitching) return;
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        };

        actions.Player.Jump.canceled += ctx =>
        {
            if (isSwitching) return;
            if (rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
            }
        };

        actions.Player.SwitchColor.performed += ctx =>
        {
            if (isSwitching) return;
            isSwitching = true;
            StartCoroutine(SwitchColor());
        };

        actions.Player.Move.performed += ctx =>
        {
            Vector2 input = ctx.ReadValue<Vector2>();
            horiz = input.x;
        };

        actions.Player.Move.canceled += ctx =>
        {
            horiz = 0f;
        };
    }

    private IEnumerator SwitchColor()
    {
        yield return new WaitForSeconds(0.8f);
        isWhiteOutlook = !isWhiteOutlook;
        BlackOutlook.SetActive(!isWhiteOutlook);
        WhiteOutlook.SetActive(isWhiteOutlook);
        yield return new WaitForSeconds(0.5f);
        isSwitching = false;
    }

    private void Update()
    {
        if (horiz != 0f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Sign(horiz) * Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }

    private void FixedUpdate()
    {
        isGrounded = groundCheck != null &&
                     Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        if (isSwitching)
        {
            rb.velocity = new Vector2(0f, 0f);
        }
        else
        {
            rb.velocity = new Vector2(horiz * speed, rb.velocity.y);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }

    private void OnDisable()
    {
        // 脚本禁用时注销事件
        var actions = InputManager.Instance?.PlayerInputActions;
        if (actions != null)
        {
            actions.Player.Jump.performed -= ctx => { };
            actions.Player.Jump.canceled -= ctx => { };
            actions.Player.SwitchColor.performed -= ctx => { };
            actions.Player.Move.performed -= ctx => { };
            actions.Player.Move.canceled -= ctx => { };
        }
    }
}
