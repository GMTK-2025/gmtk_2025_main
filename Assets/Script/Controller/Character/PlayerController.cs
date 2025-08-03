using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Audio;

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

    // 状态系统（包含死亡状态）
    [HideInInspector] public PlayerState currentState;
    [HideInInspector] public PlayerRunState runState;
    [HideInInspector] public PlayerJumpState jumpState;
    [HideInInspector] public PlayerFallState fallState;
    [HideInInspector] public PlayerVineHangingState vineHangingState;
    [HideInInspector] public PlayerHurtState hurtState;
    [HideInInspector] public PlayerWinState winState;
    [HideInInspector] public PlayerDeadState deadState; // 死亡状态


    [Header("音频设置")]
    public AudioClip runSound;
    public AudioClip jumpSound;
    public AudioClip restoreSound;
    public AudioClip ghostSound;
    public AudioClip treadSound;
    public AudioClip hurtSound;
    public AudioClip winSound;
    public AudioClip dieSound;       // 死亡音效（需在Inspector赋值）
    public AudioClip climbSound;

    private AudioSource audioSource;

    // 分身系统关联
    [Header("系统引用")]
    [SerializeField] private GhostSystem ghostSystem;

    protected virtual void Awake()
    {
        inputControl = new PlayerInputControl();
        rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>();

        // 初始化所有状态（必须包含死亡状态）
        runState = new PlayerRunState();
        jumpState = new PlayerJumpState();
        stuckState = new PlayerStuckState();
        fallState = new PlayerFallState();
        vineHangingState = new PlayerVineHangingState();
        hurtState = new PlayerHurtState();
        climbState = new PlayerClimbState();
        winState = new PlayerWinState();
        deadState = new PlayerDeadState(); // 初始化死亡状态

        if (ghostSystem == null)
        {
            ghostSystem = FindObjectOfType<GhostSystem>();
        }

        // 初始化音效组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0; // 2D音效
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

            // 重置跳跃计数（地面检测）
            if (physicsCheck.isGround)
            {
                currentJumpCount = 0;
            }

            // 无敌状态计时
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

    /// <summary>
    /// 状态切换方法
    /// </summary>
    public void SwitchState(PlayerState newState)
    {
        currentState?.OnExit(this);
        currentState = newState;
        newState?.OnEnter(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 窗帘碰撞（挂住状态）
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

        // 障碍物碰撞（受伤状态）
        if (collision.gameObject.CompareTag("Obstacle") && !isInvincible)
        {
            hurtState.SetDamageSource(collision.gameObject);
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
           currentState != vineHangingState &&
           currentState != deadState) // 死亡状态下不切换攀爬
        {
            climbState.SetClimbableObject(collision.transform);
            SwitchState(climbState);
        }
    }

    /// <summary>
    /// 触发器检测（游戏结束区域）
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("GameOverZone") && currentState != deadState) // 死亡状态下不触发胜利
        {
            Debug.Log("检测到游戏结束区域，切换到胜利状态");
            SwitchState(winState);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 藤蔓悬挂检测（死亡状态下不触发）
        if (currentState != deadState && collVine != null && collVine.IsTouching(other))
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
        // 离开藤蔓检测（死亡状态下不处理）
        if (currentState != deadState && collVine != null && !collVine.IsTouching(other) && currentState == vineHangingState)
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

    // 播放单次音效（死亡音效调用此方法）
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // 循环播放音效
    public void PlayLoopSound(AudioClip clip)
    {
        if (clip != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    // 停止循环音效
    public void StopLoopSound()
    {
        if (audioSource != null && audioSource.loop)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }
}