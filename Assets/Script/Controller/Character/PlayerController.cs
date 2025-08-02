using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using DG.Tweening.Core.Easing;

[RequireComponent(typeof(Rigidbody2D), typeof(PhysicsCheck))]
public class PlayerController : MonoBehaviour
{
    // 输入系统与基础组件
    [HideInInspector] public PlayerInputControl inputControl;
    [HideInInspector] public Vector2 inputDirection;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public PhysicsCheck physicsCheck;

    // 藤蔓碰撞体
    public Collider2D collVine;

    // 基础移动/跳跃参数
    [Header("基本参数")]
    public float speed = 5f;
    public float jumpForce = 8f;

    [Header("跳跃参数")]
    public int maxJumpCount = 2;
    [HideInInspector] public int currentJumpCount;

    // 挂住状态参数
    [Header("挂住状态参数")]
    public float stuckDuration = 2f;
    [HideInInspector] public PlayerStuckState stuckState;
    public float normalGravityScale = 4f;

    // 自定义吸附点
    [Header("吸附点设置")]
    public bool useCustomPoint;
    public Transform customStickPoint;

    // 藤蔓悬挂参数
    [Header("藤蔓悬挂参数")]
    public float vineHangDuration = 3f;
    public float vineFallSpeed = 1f;
    public float vineLinearDrag = 5f;

    // 攀爬参数
    [Header("攀爬参数")]
    public LayerMask climbLayer;
    public float climbCheckDistance = 0.5f;
    public float climbSpeed = 3f;

    // 攀爬碰撞设置
    [Header("攀爬碰撞设置")]
    public string obstacleLayerName = "Obstacle";
    public string groundLayerName = "Ground";
    public string playerLayerName = "Player";

    [HideInInspector] public PlayerClimbState climbState;

    // 无敌状态全局管理
    [Header("无敌参数")]
    public float globalInvincibleDuration = 5f;
    [HideInInspector] public bool isInvincible;
    [HideInInspector] public float invincibleTimer;

    // 受伤击退参数
    [Header("受伤参数")]
    public float hurtForceX = 3f;
    public float hurtForceY = 5f;

    // 状态系统
    [HideInInspector] public PlayerState currentState;
    [HideInInspector] public PlayerRunState runState;
    [HideInInspector] public PlayerJumpState jumpState;
    [HideInInspector] public PlayerFallState fallState;
    [HideInInspector] public PlayerVineHangingState vineHangingState;
    [HideInInspector] public PlayerHurtState hurtState;

    // 分身系统关联
    [Header("系统引用")]
    [SerializeField] private GhostSystem ghostSystem;

    protected virtual void Awake()
    {
        inputControl = new PlayerInputControl();
        rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>();

        // 初始化所有状态
        runState = new PlayerRunState();
        jumpState = new PlayerJumpState();
        stuckState = new PlayerStuckState();
        fallState = new PlayerFallState();
        vineHangingState = new PlayerVineHangingState();
        hurtState = new PlayerHurtState();
        climbState = new PlayerClimbState();

        if (ghostSystem == null)
        {
            ghostSystem = FindObjectOfType<GhostSystem>();
        }
    }

    private void OnEnable()
    {
        inputControl.Enable();
        currentState = runState;
        currentState.OnEnter(this);
        currentJumpCount = 0;
        isInvincible = false;
    }

    private void OnDisable()
    {
        inputControl.Disable();
        currentState?.OnExit(this);
    }

    private void Update()
    {
        if (currentState != null)
        {
            inputDirection = inputControl.Player.Move.ReadValue<Vector2>();
            currentState.LogicUpdate();

            if (physicsCheck.isGround)
            {
                currentJumpCount = 0;
            }

            if (isInvincible)
            {
                invincibleTimer -= Time.deltaTime;
                if (invincibleTimer <= 0)
                {
                    isInvincible = false;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        currentState?.PhysicsUpdate();
    }

    public void SwitchState(PlayerState newState)
    {
        currentState?.OnExit(this);
        currentState = newState;
        newState?.OnEnter(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 移除移动物体的碰撞检测，现在通过触发器处理

        // 窗帘碰撞
        if (collision.gameObject.CompareTag("Curtain") && currentState != stuckState)
        {
            ContactPoint2D contact = collision.GetContact(0);
            Vector2 contactWorldPos = contact.point + contact.normal * 0.1f;

            SwitchState(stuckState);
            if (useCustomPoint && customStickPoint != null)
            {
                stuckState.SetTargetPoint(customStickPoint);
            }
            else
            {
                stuckState.CreateAndSetTargetPoint(collision.transform, contactWorldPos);
            }
        }

        // 障碍物碰撞
        if (collision.gameObject.CompareTag("Obstacle") && !isInvincible)
        {
            SwitchState(hurtState);
            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(knockbackDirection.x * hurtForceX, hurtForceY), ForceMode2D.Impulse);
        }

        // 攀爬检测
        if (IsClimbable(collision.gameObject) &&
           currentState != climbState &&
           currentState != hurtState &&
           currentState != stuckState &&
           currentState != vineHangingState)
        {
            climbState.SetClimbableObject(collision.transform);
            SwitchState(climbState);
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("GameOverZone"))
        {
            Debug.Log("游戏结束");
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (collVine != null && collVine.IsTouching(other))
        {
            if (currentState != vineHangingState && currentState != stuckState)
            {
                SwitchState(vineHangingState);
            }
            else if (currentState == vineHangingState)
            {
                vineHangingState.UpdateVineContact(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (collVine != null && !collVine.IsTouching(other) && currentState == vineHangingState)
        {
            vineHangingState.UpdateVineContact(false);
        }
    }

    public void StartCustomCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    public GhostSystem GetGhostSystem()
    {
        if (ghostSystem == null)
        {
            ghostSystem = FindObjectOfType<GhostSystem>();
        }
        return ghostSystem;
    }

    private bool IsClimbable(GameObject obj)
    {
        return (climbLayer.value & (1 << obj.layer)) != 0;
    }
}