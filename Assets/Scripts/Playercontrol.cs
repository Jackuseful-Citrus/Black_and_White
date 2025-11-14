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

    private PlayerInputActions playerInputActions;

    Rigidbody2D rb;
    float horiz = 0f;
    bool isGrounded;

    public GameObject BlackOutlook;
    public GameObject WhiteOutlook;
    private bool isWhiteOutlook = false;

    public bool isWhite => isWhiteOutlook;
    public bool isBlack => !isWhiteOutlook;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 防止旋转
        BlackOutlook.SetActive(true);
        WhiteOutlook.SetActive(false);
    }

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions
            .Player.Jump.performed += ctx =>
        {
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        };
        playerInputActions.Player.Jump.canceled += ctx =>
        {
            if (rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
            }
        };

        playerInputActions.Player.SwitchColor.performed += ctx =>
        {
            isWhiteOutlook = !isWhiteOutlook;
            BlackOutlook.SetActive(!isWhiteOutlook);
            WhiteOutlook.SetActive(isWhiteOutlook);
        };

        playerInputActions.Player.Move.performed += ctx =>
        {
            Vector2 input = ctx.ReadValue<Vector2>();
            horiz = input.x;
        };
        playerInputActions.Player.Move.canceled += ctx =>
        {
            horiz = 0f;
        };

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

        rb.velocity = new Vector2(horiz * speed, rb.velocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}
