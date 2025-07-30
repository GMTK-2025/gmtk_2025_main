using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public override void OnEnter(PlayerController player)
    {
        this.player = player;
        player.currentJumpCount++;

        // 仅在地面跳跃时重置速度（防止空中二段跳干扰）
        if (player.physicsCheck.isGround)
        {
            player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, 0);
        }

        // 施加跳跃力（确保使用Impulse模式）
        player.rb.AddForce(Vector2.up * player.jumpForce, ForceMode2D.Impulse);

        // Debug.Log($"Jumped! Current jump count: {player.currentJumpCount}");
    }

    public override void LogicUpdate()
    {
        // 下落时切换到FallState（而不是直接回RunState）
        if (player.rb.linearVelocity.y < 0)
        {
            player.SwitchState(player.fallState);
        }
        // 二段跳检测
        else if (player.inputControl.Player.Jump.WasPressedThisFrame() &&
                player.currentJumpCount < player.maxJumpCount)
        {
            player.SwitchState(new PlayerJumpState());
        }
    }

    public override void PhysicsUpdate() { }
    public override void OnExit() { }
}