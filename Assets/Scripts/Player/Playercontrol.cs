using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControl : MonoBehaviour
{
    public event System.Action<bool> OnSwitchStart; // bool: toWhite

    [SerializeField] float speed = 6f;
    [SerializeField] float jumpForce = 12f;
    [SerializeField] float jumpCutMultiplier = 0.1f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundRadius = 0.1f;
    [SerializeField] float groundCheckDistance = 0.2f; // downwards ground check only
    [SerializeField] float wallCheckDistance = 0.2f;  // horizontal wall check distance
    [SerializeField] LayerMask groundLayer;
    [SerializeField] GameObject Scythe;
    [SerializeField] GameObject LightBallSpawner;

    private ScytheScript scytheScript;
    private LightBallSpawnerScript lightBallSpawnerScript;

    Rigidbody2D rb;
    public float horiz = 0f;
    public bool isGrounded;
    public bool isAttacking = false;
    
    public GameObject BlackOutlook;
    public GameObject WhiteOutlook;
    private bool isWhiteOutlook = false;
    private bool isSwitching = false;

    public bool isWhite => isWhiteOutlook;
    public bool isBlack => !isWhiteOutlook;
    private PlayerAnimationController animCtrl;
    private bool isInAttackRecovery = false;
    private bool isInAttackProgress = false;
    private bool isOnWall = false;
    private bool wasOnWall = false;
    private int airJumpsRemaining = 0;
    [SerializeField] int maxAirJumpsFromWall = 1000;
    
    private float speedMultiplier = 1f;

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    private void Start()
    {
        scytheScript = Scythe?.GetComponent<ScytheScript>();
        lightBallSpawnerScript = LightBallSpawner?.GetComponent<LightBallSpawnerScript>();

        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        BlackOutlook.SetActive(true);
        WhiteOutlook.SetActive(false);
        animCtrl = GetComponent<PlayerAnimationController>(); 

        var actions = InputManager.Instance.PlayerInputActions;

        actions.Player.Attack.performed += ctx =>
        {
            if (!isSwitching){
                isAttacking = true;
                }
        };

        actions.Player.Attack.canceled += ctx =>
        {
            isAttacking = false;
        };

        actions.Player.Jump.performed += ctx =>
        {
            AttemptJump();
        };

        actions.Player.Jump.canceled += ctx =>
        {
            //防止传送到新场景之后继续调用原本的rb
            if (rb == null || isSwitching)return;
            if (rb.velocity.y > 0f && !isSwitching)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
            }
        };

        actions.Player.SwitchColor.performed += ctx =>
        {
            //防止传送到新场景之后继续调用原本的rb
            if (rb == null || isSwitching)return;
            isSwitching = true;
            if (animCtrl != null)
            {
                bool toWhite = !isWhiteOutlook;     // current black means switching to white
                animCtrl.PlaySwitch(toWhite);
            }
            bool switchToWhite = !isWhiteOutlook;
            OnSwitchStart?.Invoke(switchToWhite);
            StartCoroutine(SwitchColor(switchToWhite));
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

    private void AttemptJump()
    {
        if (isSwitching || rb == null) return;

        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            airJumpsRemaining = maxAirJumpsFromWall;
        }
        else if (airJumpsRemaining > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            airJumpsRemaining--;
        }
    }

    private IEnumerator SwitchColor(bool toWhite)
    {
        yield return new WaitForSeconds(0.8f);
        isWhiteOutlook = toWhite;
        BlackOutlook.SetActive(!isWhiteOutlook);
        WhiteOutlook.SetActive(isWhiteOutlook);
        yield return new WaitForSeconds(0.5f);
        isSwitching = false;
    }

    private void Update()
    {
        if (!isActiveAndEnabled) return;
        if (horiz != 0f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Sign(horiz) * Mathf.Abs(s.x);
            transform.localScale = s;
        }
        if (isBlack)
        {
            isInAttackRecovery = scytheScript != null && scytheScript.inAttackRecovery;
            isInAttackProgress = scytheScript != null && scytheScript.inAttackProgress;
        }else if (isWhite)
        {
            isInAttackRecovery = lightBallSpawnerScript != null && lightBallSpawnerScript.inAttackRecovery;
        }

        var tp = LogicScript.Instance != null ? LogicScript.Instance.ConsumePendingTeleportPoint() : Vector3.zero;
        if (tp != Vector3.zero)
        {
            transform.position = tp;
        }
    }

    private void FixedUpdate()
    {
        if (!isActiveAndEnabled) return;
        wasOnWall = isOnWall;
        isOnWall = false;

        if (groundCheck != null)
        {
            isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        }

        if (!isGrounded)
        {
            float dir = Mathf.Abs(horiz) > 0.01f ? Mathf.Sign(horiz) : Mathf.Sign(transform.localScale.x);
            Vector2 origin = transform.position;
            if (Physics2D.Raycast(origin, Vector2.right * dir, wallCheckDistance, groundLayer))
            {
                isOnWall = true;
            }
        }

        if (isGrounded)
        {
            airJumpsRemaining = maxAirJumpsFromWall;
        }
        else if (isOnWall && !wasOnWall)
        {
            airJumpsRemaining = Mathf.Max(airJumpsRemaining, 1);
        }

        if (isSwitching)
        {
            rb.velocity = new Vector2(0f, 0f);
        }
        else if ((isAttacking && !isInAttackRecovery)|| isInAttackProgress){
            rb.velocity = new Vector2(rb.velocity.x *0.9f, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(horiz * speed * speedMultiplier, rb.velocity.y);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Vector3 start = groundCheck.position;
            Vector3 end = start + Vector3.down * groundCheckDistance;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, groundRadius);
        }

        Gizmos.color = Color.yellow;
        Vector3 wallStart = transform.position;
        Vector3 wallEnd = wallStart + Vector3.right * wallCheckDistance;
        Gizmos.DrawLine(wallStart, wallEnd);
        Gizmos.DrawWireSphere(wallEnd, 0.05f);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;
        isSwitching = false;
        isInAttackRecovery = false;
        isInAttackProgress = false;
        horiz = 0f;
        isOnWall = false;
        wasOnWall = false;
        airJumpsRemaining = 0;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
}
