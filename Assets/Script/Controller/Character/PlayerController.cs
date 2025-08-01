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

    [Header("��������")]
    public float speed;
    public float jumpForce;

    [Header("��Ծ����")]
    public int maxJumpCount = 2;
    [HideInInspector] public int currentJumpCount;

    [Header("��ס״̬����")]
    public float stuckDuration = 2f;
    [HideInInspector] public PlayerState stuckState;

    [Header("�������Ҳ���")]
    public float vineHangDuration = 3f; // ���ҳ���ʱ��
    public float vineFallSpeed = 1f; // ����ʱ�������ٶ�
    public float vineLinearDrag = 5f; // ����ʱ��Ħ����

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

        // ��ʼ������״̬
        runState = new PlayerRunState();
        jumpState = new PlayerJumpState();
        stuckState = new PlayerStuckState();
        fallState = new PlayerFallState(); 
        vineHangingState = new PlayerVineHangingState();
    }

    private void Start()
    {

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
        // ���ָ����collVine�Ƿ��뵱ǰother��ײ���Ӵ�
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
        // ���ָ����collVine�Ƿ��������other��ײ���ĽӴ�
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