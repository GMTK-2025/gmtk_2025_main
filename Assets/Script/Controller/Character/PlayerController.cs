using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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
    public LayerMask climbLayer; // 攀爬层（拖入可攀爬物体所在层）
    public float climbCheckDistance = 0.5f; // 攀爬检测距离
    public float climbSpeed = 3f; // 攀爬速度参数

    // 攀爬碰撞设置 - 新增
    [Header("攀爬碰撞设置")]
    public string obstacleLayerName = "Obstacle";
    public string groundLayerName = "Ground";
    public string playerLayerName = "Player";

    [HideInInspector] public PlayerClimbState climbState; // 攀爬状态

    // 无敌状态全局管理
    [Header("无敌参数")]
    public float globalInvincibleDuration = 5f; // 无敌持续时间
    [HideInInspector] public bool isInvincible; // 当前是否无敌
    [HideInInspector] public float invincibleTimer; // 无敌计时器

    // 状态系统：所有状态引用
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
        // 绑定基础组件
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
        climbState = new PlayerClimbState(); // 初始化攀爬状态

        // 自动查找GhostSystem
        if (ghostSystem == null)
        {
            ghostSystem = FindObjectOfType<GhostSystem>();
            if (ghostSystem == null)
            {
                Debug.LogWarning("未找到GhostSystem，请在Inspector手动指定");
            }
        }
    }

    private void OnEnable()
    {
        inputControl.Enable();
        currentState = runState;
        currentState.OnEnter(this);
        currentJumpCount = 0;
        isInvincible = false; // 初始禁用无敌
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
            // 更新输入方向
            inputDirection = inputControl.Player.Move.ReadValue<Vector2>();
            currentState.LogicUpdate();

            // 落地重置跳跃次数
            if (physicsCheck.isGround)
            {
                currentJumpCount = 0;
            }

            // 全局无敌计时器（所有状态共享）
            if (isInvincible)
            {
                invincibleTimer -= Time.deltaTime;
                if (invincibleTimer <= 0)
                {
                    isInvincible = false;
                    Debug.Log("全局无敌状态结束");
                }
            }
        }
    }

    private void FixedUpdate()
    {
        currentState?.PhysicsUpdate();
    }

    // 状态切换核心方法
    public void SwitchState(PlayerState newState)
    {
        currentState?.OnExit(this);
        currentState = newState;
        newState?.OnEnter(this);
    }

    // 碰撞检测逻辑
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 窗帘碰撞 → 挂住状态
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

        // 障碍物碰撞 → 受伤状态（带无敌判断）
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (!isInvincible)
            {
                Debug.Log("非无敌状态，触发受伤");
                SwitchState(hurtState);
            }
            else
            {
                Debug.Log("无敌状态，忽略伤害");
            }
        }

        // 碰到攀爬层物体自动进入攀爬状态
        if (IsClimbable(collision.gameObject))
        {
            // 只有在特定状态下才能进入攀爬状态
            if (currentState != climbState &&
                currentState != hurtState &&
                currentState != stuckState &&
                currentState != vineHangingState)
            {
                Debug.Log("碰撞到攀爬物，进入攀爬状态");
                // 设置攀爬物体引用
                climbState.SetClimbableObject(collision.transform);
                SwitchState(climbState);
            }
        }
    }

    // 藤蔓触发检测
    private void OnTriggerStay2D(Collider2D other)
    {
        if (collVine != null && collVine.IsTouching(other))
        {
            if (currentState != vineHangingState && currentState != stuckState)
            {
                SwitchState(vineHangingState);
            }
            else if (currentState == vineHangingState && vineHangingState != null)
            {
                vineHangingState.UpdateVineContact(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (collVine != null && !collVine.IsTouching(other) && currentState == vineHangingState && vineHangingState != null)
        {
            vineHangingState.UpdateVineContact(false);
        }
    }

    // 协程启动方法
    public void StartCustomCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    // 获取GhostSystem引用
    public GhostSystem GetGhostSystem()
    {
        if (ghostSystem == null)
        {
            ghostSystem = FindObjectOfType<GhostSystem>();
        }
        return ghostSystem;
    }

    // 检测物体是否为可攀爬物
    private bool IsClimbable(GameObject obj)
    {
        // 检查物体是否在攀爬层中
        return (climbLayer.value & (1 << obj.layer)) != 0;
    }

}
