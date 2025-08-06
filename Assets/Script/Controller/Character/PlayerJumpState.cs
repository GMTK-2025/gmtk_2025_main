using UnityEngine;

public class PlayerJumpState : PlayerState
{
    // 跳跃输入缓冲时间（允许输入提前一点）
    private const float JumpInputBuffer = 0.15f;
    // Coyote时间（离开地面后短时间仍可跳跃）
    private const float CoyoteTime = 0.1f;

    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private bool _hasJumpedThisFrame;

    public override void OnEnter(PlayerController player)
    {
        this._player = player;

        // 播放跳跃音效
        player.PlaySound(player.jumpSound);

        // 重置垂直速度，确保跳跃高度一致
        player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, 0);

        // 施加跳跃力（使用直接赋值而非AddForce，避免叠加）
        player.rb.linearVelocity = new Vector2(
            player.rb.linearVelocity.x,
            player.jumpForce
        );

        // 增加跳跃计数
        player.currentJumpCount++;
        _hasJumpedThisFrame = true;
    }

    public override void LogicUpdate()
    {
        // 处理Coyote时间和跳跃缓冲
        UpdateJumpTimers();

        // 下落时切换到下落状态
        if (_player.rb.linearVelocity.y < 0)
        {
            _player.SwitchState(_player.fallState);
            return;
        }

        // 二段跳检测（限制最大跳跃次数为2，且处理输入缓冲）
        bool canDoubleJump = _player.currentJumpCount < _player.maxJumpCount - 1; // 修改这里，确保最多只能跳2次
        bool jumpInputPressed = _player.inputControl.Player.Jump.WasPressedThisFrame();
        bool jumpBufferActive = _jumpBufferTimer > 0;

        if ((jumpInputPressed || jumpBufferActive) && canDoubleJump && !_hasJumpedThisFrame)
        {
            // 消耗缓冲，执行二段跳
            _jumpBufferTimer = 0;
            _player.SwitchState(new PlayerJumpState());
        }
    }

    private void UpdateJumpTimers()
    {
        // 更新Coyote时间（仅在地面时重置）
        if (_player.physicsCheck.isGround)
        {
            _coyoteTimer = CoyoteTime;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
        }

        // 更新跳跃输入缓冲
        if (_player.inputControl.Player.Jump.WasPressedThisFrame())
        {
            _jumpBufferTimer = JumpInputBuffer;
        }
        else
        {
            _jumpBufferTimer -= Time.deltaTime;
        }

        // 每帧重置单次跳跃标记
        _hasJumpedThisFrame = false;
    }

    public override void PhysicsUpdate() { }

    public override void OnExit(PlayerController player) { }
}