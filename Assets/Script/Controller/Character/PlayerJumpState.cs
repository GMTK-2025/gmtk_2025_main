using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public override void OnEnter(PlayerController player)
    {
        this.player = player;
        // 施加跳跃力（仅在地面上时）
        if (player.physicsCheck.isGround)
        {
            player.rb.AddForce(
                player.transform.up * player.jumpForce,
                ForceMode2D.Impulse
            );
        }
    }

    public override void LogicUpdate()
    {
        // 落地后切换回跑步状态
        if (player.physicsCheck.isGround)
        {
            player.SwitchState(player.runState);
        }
    }

    public override void PhysicsUpdate()
    {
      
    }

    public override void OnExit()
    {
        
    }
}
