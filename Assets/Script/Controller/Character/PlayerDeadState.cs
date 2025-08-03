using UnityEngine;

public class PlayerDeadState : PlayerState
{
    public override void OnEnter(PlayerController player)
    {
        this.player = player;

        // 禁用输入控制（包括移动和跳跃）
        player.inputControl.Player.Move.Disable();
        player.inputControl.Player.Jump.Disable();

        // 停止物理运动
        player.rb.linearVelocity = Vector2.zero;
        player.rb.simulated = false;

        // 播放死亡音效（单次播放）
        player.PlaySound(player.dieSound);

        Debug.Log("Player entered dead state");
    }

    public override void LogicUpdate()
    {
        // 死亡状态无需逻辑更新，可添加游戏重启逻辑
    }

    public override void PhysicsUpdate()
    {
        // 死亡状态无需物理更新
    }

    public override void OnExit(PlayerController player)
    {
        // 退出死亡状态时恢复控制
        player.inputControl.Player.Move.Enable();
        player.inputControl.Player.Jump.Enable();
        player.rb.simulated = true;
    }
}