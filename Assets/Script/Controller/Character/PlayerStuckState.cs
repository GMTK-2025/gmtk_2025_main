using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStuckState : PlayerState
{
    private float stuckTimer;
    private Transform anchorPoint;
    private bool isUsingTempPoint;
    private float playerHeight;
    private Vector3 positionOffset;
    private bool hasHandledJumpInput;

    // 设置锚点（关联到场景中固定点）
    public void SetTargetPoint(Transform point)
    {
        if (player == null || point == null) return;
        CleanupTempPoint();
        anchorPoint = point;
        isUsingTempPoint = false;
        CalculatePlayerHeight();
        CalculateOffset();
    }

    // 动态创建锚点（用于临时挂载点）
    public void CreateAndSetTargetPoint(Transform parent, Vector2 localPos)
    {
        if (player == null || parent == null) return;
        CleanupTempPoint();

        GameObject tempPoint = new GameObject("TempAnchorPoint");
        tempPoint.transform.SetParent(parent);
        tempPoint.transform.localPosition = localPos;

        anchorPoint = tempPoint.transform;
        isUsingTempPoint = true;
        CalculatePlayerHeight();
        CalculateOffset();
    }

    // 计算角色高度（用于锚点偏移）
    private void CalculatePlayerHeight()
    {
        if (player == null) return;
        Collider2D collider = player.GetComponent<Collider2D>();
        playerHeight = collider != null ? collider.bounds.size.y : 1f;
    }

    // 计算锚点偏移
    private void CalculateOffset()
    {
        positionOffset = new Vector3(0, -playerHeight, 0);
    }


    private void CleanupTempPoint()
    {
        if (isUsingTempPoint && anchorPoint != null)
        {
            //Destroy(anchorPoint.gameObject);
        }
    }

    // 进入挂住状态时初始化
    public override void OnEnter(PlayerController player)
    {
        this.player = player;
        stuckTimer = player.stuckDuration;
        hasHandledJumpInput = false;

        // 锁定刚体为运动学模式（暂停物理模拟）
        player.rb.bodyType = RigidbodyType2D.Kinematic;
        player.rb.gravityScale = 0;
        player.rb.linearVelocity = Vector2.zero;
    }

    // 每帧逻辑更新（检测输入、超时）
    public override void LogicUpdate()
    {
        if (player == null) return;

        // 检测跳跃输入（按空格离开挂住状态）
        bool jumpPressed = player.inputControl.Player.Jump.IsPressed();
        if (jumpPressed && !hasHandledJumpInput)
        {
            hasHandledJumpInput = true;
            EscapeStuck();
            return;
        }

        // 超时自动离开挂住状态
        stuckTimer -= Time.deltaTime;
        if (stuckTimer <= 0)
        {
            EscapeStuck();
        }
    }

    // 物理更新（保持角色位置与锚点同步）
    public override void PhysicsUpdate()
    {
        if (player != null && anchorPoint != null)
        {
            player.transform.position = anchorPoint.position + positionOffset;
            player.rb.linearVelocity = Vector2.zero;
        }
    }

    // 离开挂住状态（恢复物理、施加力、切换状态）
    private void EscapeStuck()
    {
        if (player == null) return;

        // 恢复刚体为动态模式（启用物理模拟）
        player.rb.bodyType = RigidbodyType2D.Dynamic;
        player.rb.gravityScale = player.normalGravityScale;

        // 施加离开力（让角色掉落/弹开）
        Vector2 escapeVelocity = Vector2.up * player.jumpForce * 0.4f;
        if (player.inputDirection.x != 0)
        {
            escapeVelocity.x = player.inputDirection.x * player.speed * 0.6f;
        }
        player.rb.linearVelocity = escapeVelocity;

        // 切换状态（落地则跑，否则下落）
        player.SwitchState(player.physicsCheck.isGround ? player.runState : player.fallState);
    }

    // 离开状态时清理
    public override void OnExit(PlayerController player)
    {
        CleanupTempPoint();
        anchorPoint = null;
        hasHandledJumpInput = false;
    }
}