using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PhysicsCheck))]
public class PlayerController : MonoBehaviour
{
    [HideInInspector] public PlayerInputControl inputControl;
    [HideInInspector] public Vector2 inputDirection;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public PhysicsCheck physicsCheck;

    [Header("基本参数")]
    public float speed;
    public float jumpForce;

    [HideInInspector] public PlayerState currentState;
    [HideInInspector] public PlayerState runState;
    [HideInInspector] public PlayerState jumpState;

    protected virtual void Awake()
    {
        inputControl = new PlayerInputControl();
        rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>();

        // 初始化状态
        runState = new PlayerRunState();
        jumpState = new PlayerJumpState();
    }

    private void OnEnable()
    {
        inputControl.Enable();
        // 初始状态设为跑步状态
        currentState = runState;
        currentState.OnEnter(this);
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
}
