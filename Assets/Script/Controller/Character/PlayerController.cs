using System;
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
    [HideInInspector] public PlayerStuckState stuckState;
    public float normalGravityScale = 4f;

    [Header("吸附点设置")]
    public bool useCustomPoint;
    public Transform customStickPoint;

    [Header("藤蔓悬挂参数")]
    public float vineHangDuration = 3f;
    public float vineFallSpeed = 1f;
    public float vineLinearDrag = 5f;

    [HideInInspector] public PlayerState currentState;
    [HideInInspector] public PlayerRunState runState;
    [HideInInspector] public PlayerJumpState jumpState;
    [HideInInspector] public PlayerFallState fallState;
    [HideInInspector] public PlayerVineHangingState vineHangingState;

    protected virtual void Awake()
    {
        inputControl = new PlayerInputControl();
        rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>();

        // 初始化状态
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
        if (currentState != null)
        {
            currentState.OnExit(this);
        }
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
        }
    }

    private void FixedUpdate()
    {
        if (currentState != null)
        {
            currentState.PhysicsUpdate();
        }
    }

    public void SwitchState(PlayerState newState)
    {
        if (currentState != null)
        {
            currentState.OnExit(this);
        }
        currentState = newState;
        if (newState != null)
        {
            newState.OnEnter(this);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
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
    }

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

    public void StartCustomCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
}