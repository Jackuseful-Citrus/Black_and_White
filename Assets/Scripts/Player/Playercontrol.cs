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

        // 使用全局 InputManager 实例
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
            if (isGrounded && !isSwitching)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        };

        actions.Player.Jump.canceled += ctx =>
        {
            if (rb.velocity.y > 0f && !isSwitching)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
            }
        };

        actions.Player.SwitchColor.performed += ctx =>
        {
            if (isSwitching) return;
            isSwitching = true;
            if (animCtrl != null)
            {
                bool toWhite = !isWhiteOutlook;     // 当前是黑的话就是要切到白
                animCtrl.PlaySwitch(toWhite);
            }
            bool switchToWhite = !isWhiteOutlook;
            OnSwitchStart?.Invoke(switchToWhite);   // 通知镜像同步播放切换
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
    }

    private void FixedUpdate()
    {
        if (!isActiveAndEnabled) return;
        isGrounded = groundCheck != null &&
                     Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

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
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }

    private void OnDisable()
    {
        // 停止所有协程以防止在禁用时继续执行
        StopAllCoroutines();
        //重置状态
        isAttacking = false;
        isSwitching = false;
        isInAttackRecovery = false;
        isInAttackProgress = false;
        horiz = 0f;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
}
