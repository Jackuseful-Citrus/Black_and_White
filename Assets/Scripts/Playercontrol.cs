using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Playercontrol : MonoBehaviour
{
    [SerializeField] float speed = 6f;
    [SerializeField] float jumpForce = 8f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundRadius = 0.1f;
    [SerializeField] LayerMask groundLayer;

    Rigidbody2D rb;
    float horiz;
    bool isGrounded;

    public GameObject BlackOutlook;
    public GameObject WhiteOutlook;
    private bool isWhiteOutlook = false;

    public bool isWhite => isWhiteOutlook;
    public bool isBlack => !isWhiteOutlook;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 防止旋转
        BlackOutlook.SetActive(true);
        WhiteOutlook.SetActive(false);
    }

    void Update()
    {
        horiz = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horiz = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horiz = 1f;


        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        if (horiz != 0f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Sign(horiz) * Mathf.Abs(s.x);
            transform.localScale = s;
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            isWhiteOutlook = !isWhiteOutlook;
            BlackOutlook.SetActive(!isWhiteOutlook);
            WhiteOutlook.SetActive(isWhiteOutlook);
        }
    }

    void FixedUpdate()
    {
        isGrounded = groundCheck != null &&
                     Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        rb.velocity = new Vector2(horiz * speed, rb.velocity.y);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}
