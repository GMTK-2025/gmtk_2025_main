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
    [HideInInspector] public PlayerAnimationManager animManager;

    // 藤蔓碰撞体
    public Collider2D collVine;

    // 基础移动/跳跃参数
    [Header("基本参数")]
    public float speed = 5f;
    public float jumpForce = 8f;

    [Header("跳跃参数")]
    public int maxJumpCount = 2; // 修改为2表示最多跳2次（初始跳+1次二段跳）
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
    [HideInInspector] public PlayerWinState winState;
    [HideInInspector] public PlayerDeadState deadState;

    [Header("音频设置")]
    public AudioClip runSound;
    public AudioClip jumpSound;
    public AudioClip restoreSound;
    public AudioClip ghostSound;
    public AudioClip treadSound;
    public AudioClip hurtSound;
    public AudioClip winSound;
    public AudioClip dieSound;
    public AudioClip climbSound;
    [Range(0f, 1f)] public float hurtSoundVolume = 0.7f;

    private AudioSource audioSource;

    // 分身系统关联
    [Header("系统引用")]
    [SerializeField] private GhostSystem ghostSystem;

    protected virtual void Awake()
    {
        // 初始化输入系统
        inputControl = new PlayerInputControl();

        // 获取或添加必要组件
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("缺少 Rigidbody2D 组件，已自动添加", this);
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        physicsCheck = GetComponent<PhysicsCheck>();
        if (physicsCheck == null)
        {
            Debug.LogError("缺少 PhysicsCheck 组件，需手动添加或确保已有该组件实现地面检测等逻辑", this);
        }

        // 初始化状态（防护：若状态类未正确实例化，这里至少赋空或尝试新建）
        runState = new PlayerRunState();
        jumpState = new PlayerJumpState();
        stuckState = new PlayerStuckState();
        fallState = new PlayerFallState();
        vineHangingState = new PlayerVineHangingState();
        hurtState = new PlayerHurtState();
        climbState = new PlayerClimbState();
        winState = new PlayerWinState();
        deadState = new PlayerDeadState();

        // 查找或赋值 GhostSystem
        if (ghostSystem == null)
        {
            ghostSystem = FindObjectOfType<GhostSystem>();
            if (ghostSystem == null)
            {
                Debug.LogWarning("场景中未找到 GhostSystem，若无需分身功能可忽略，否则需添加该组件", this);
            }
        }

        // 初始化音效组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0;
            Debug.Log("自动添加 AudioSource 组件", this);
        }
        else
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0;
        }

        // 初始化动画管理组件
        animManager = GetComponent<PlayerAnimationManager>();
        if (animManager == null)
        {
            animManager = gameObject.AddComponent<PlayerAnimationManager>();
            Debug.LogWarning("自动添加 PlayerAnimationManager 组件", this);
        }

        // 初始化刚体参数（确保物理表现稳定）
        rb.gravityScale = normalGravityScale;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void OnEnable()
    {
        inputControl.Enable();
        // 防护：确保 currentState 初始化，这里先默认给 runState，若 runState 没正确创建，至少不会空引用
        currentState = runState ?? new PlayerRunState();
        currentState?.OnEnter(this);
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
        if (currentState == null)
        {
            Debug.LogWarning("currentState 为空，可能状态初始化异常", this);
            return;
        }

        inputDirection = inputControl.Player.Move.ReadValue<Vector2>();
        currentState.LogicUpdate();

        // 地面检测：重置跳跃计数（防护：physicsCheck 为空时不执行，避免空引用）
        if (physicsCheck != null && physicsCheck.isGround)
        {
            currentJumpCount = 0;
        }

        // 无敌状态计时（防护：isInvincible 为 true 且 timer 逻辑执行）
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0)
            {
                isInvincible = false;
            }
        }
    }

    private void FixedUpdate()
    {
        currentState?.PhysicsUpdate();
    }

    /// <summary>
    /// 状态切换方法（增加防护，newState 为空时不切换）
    /// </summary>
    public void SwitchState(PlayerState newState)
    {
        if (newState == null)
        {
            Debug.LogWarning("尝试切换到空状态，忽略此次切换", this);
            return;
        }
        currentState?.OnExit(this);
        currentState = newState;
        newState.OnEnter(this);
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
            if (hurtState == null)
            {
                hurtState = new PlayerHurtState();
            }
            hurtState.SetDamageSource(collision.gameObject);
            SwitchState(hurtState);

            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(knockbackDirection.x * hurtForceX, hurtForceY), ForceMode2D.Impulse);
        }

        // 攀爬检测（增加对 climbState 等的防护）
        if (IsClimbable(collision.gameObject) &&
           currentState != climbState &&
           currentState != hurtState &&
           currentState != stuckState &&
           currentState != vineHangingState &&
           currentState != deadState)
        {
            if (climbState == null)
            {
                climbState = new PlayerClimbState();
            }
            climbState.SetClimbableObject(collision.transform);
            SwitchState(climbState);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("GameOverZone") && currentState != deadState)
        {
            Debug.Log("检测到游戏结束区域，切换到胜利状态");
            SwitchState(winState);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 藤蔓悬挂检测（防护：collVine 为空或 currentState 异常时不处理）
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
        // 离开藤蔓检测（防护：collVine 为空或 currentState 异常时不处理）
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

    // 播放带音量的音效
    public void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    // 播放默认音量音效
    public void PlaySound(AudioClip clip)
    {
        PlaySound(clip, 1f);
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