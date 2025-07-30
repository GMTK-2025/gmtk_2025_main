using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(PhysicsCheck))]
public class PlayerController : MonoBehaviour
{
    [HideInInspector] public PlayerInputControl inputControl;
    [HideInInspector] public Vector2 inputDirection;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public PhysicsCheck physicsCheck;

    public Collider2D collVine;

    [Header("基本参数")]
    public float speed;
    public float jumpForce;

    [Header("跳跃参数")]
    public int maxJumpCount = 2;
    [HideInInspector] public int currentJumpCount;

    [Header("挂住状态参数")]
    public float stuckDuration = 2f;
    [HideInInspector] public PlayerState stuckState;

    [Header("藤蔓悬挂参数")]
    public float vineHangDuration = 3f; // 悬挂持续时间
    public float vineFallSpeed = 1f; // 悬挂时的下落速度
    public float vineLinearDrag = 5f; // 悬挂时的摩擦力

    [HideInInspector] public PlayerState currentState;
    [HideInInspector] public PlayerState runState;
    [HideInInspector] public PlayerState jumpState;
    [HideInInspector] public PlayerState fallState;
    [HideInInspector] public PlayerState vineHangingState;

   
    

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
    }

    private void OnEnable()
    {
        inputControl.Enable();
        currentState = runState;
        currentState.OnEnter(this);
        currentJumpCount = 0;
    }

    private void OnDisable()
    {
        inputControl.Disable();
        currentState.OnExit();
    }

    private void Update()
    {
        inputDirection = inputControl.Player.Move.ReadValue<Vector2>();
        currentState.LogicUpdate();

        if (physicsCheck.isGround)
        {
            currentJumpCount = 0;
        }
    }

    private void FixedUpdate()
    {
        currentState.PhysicsUpdate();
    }

    public void SwitchState(PlayerState newState)
    {
        currentState.OnExit();
        currentState = newState;
        currentState.OnEnter(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Curtain") && currentState != stuckState)
        {
            if (!physicsCheck.isGround)
            {
                SwitchState(stuckState);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 检查指定的collVine是否与当前other碰撞器接触
        if (collVine != null && collVine.IsTouching(other))
        {
            if (currentState != vineHangingState && currentState != stuckState)
            {
                SwitchState(vineHangingState);
            }
            else if (currentState == vineHangingState)
            {
                (vineHangingState as PlayerVineHangingState).UpdateVineContact(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 检查指定的collVine是否结束了与other碰撞器的接触
        if (collVine != null && !collVine.IsTouching(other) && currentState == vineHangingState)
        {
            (vineHangingState as PlayerVineHangingState).UpdateVineContact(false);
        }
    }

    public void StartCustomCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
}