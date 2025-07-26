using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
  public PlayerInputControl inputControl;
    public Vector2 inputDirection;
    private Rigidbody2D rb;
    private PhysicsCheck physicsCheck;
    [Header("基本参数")]
    public float speed;
    public float jumpForce;
    private void Awake()
    {
        inputControl = new PlayerInputControl();
        rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>();
        inputControl.Player.Jump.started += Jump;
    }

  

    private void OnEnable()
    {
        inputControl.Enable();
    }
    private void OnDisable()
    {
        inputControl.Disable();
    }
    private void Update()
    {
        inputDirection = inputControl.Player.Move.ReadValue<Vector2>();
    }
    private void FixedUpdate()
    {
        Move();
    }
    public void Move()
    {
        rb.linearVelocity = new Vector2(inputDirection.x * speed * Time.deltaTime, rb.linearVelocity.y);

        if (inputDirection.x != 0)
        {
            // 只修改父物体的x缩放
            int faceDir = inputDirection.x > 0 ? 1 : -1;
            transform.localScale = new Vector3(faceDir, 1, 1);

            // 对每个子物体应用相反的x缩放来抵消父物体的影响
            foreach (Transform child in transform)
            {
                child.localScale = new Vector3(1.0f / faceDir, 2, 1);
            }
        }
    }
    private void Jump(InputAction.CallbackContext context)
    {
        // Debug.Log("JUMP");
        if (physicsCheck.isGround)
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
    }
}
