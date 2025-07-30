using UnityEngine;

public class PlayerStuckState : PlayerState
{
    private float stuckTimer;
   

    public override void OnEnter(PlayerController player)
    {
        this.player = player;
        stuckTimer = player.stuckDuration;

        // 彻底冻结角色，防止任何外力影响
        player.rb.linearVelocity = Vector2.zero;
        player.rb.gravityScale = 0;
        player.rb.bodyType = RigidbodyType2D.Kinematic; 
        // Debug.Log("Entered Stuck State");
    }

    public override void LogicUpdate()
    {
        stuckTimer -= Time.deltaTime;

        // 挣脱条件：计时结束或手动按键挣脱（可选）
        if (stuckTimer <= 0 || player.inputControl.Player.Jump.WasPressedThisFrame())
        {
            EscapeStuck();
        }
    }

    private void EscapeStuck()
    {
        // 恢复物理状态，但确保速度归零
        player.rb.bodyType = RigidbodyType2D.Dynamic;
        player.rb.gravityScale = 4;
        player.rb.linearVelocity = Vector2.zero; 

        // 根据是否落地切换状态
        if (player.physicsCheck.isGround)
        {
            player.SwitchState(player.runState);
        }
        else
        {
            // 直接进入下落状态，而不是JumpState，避免额外受力
            player.SwitchState(player.fallState);
        }

        // Debug.Log("Escaped Stuck State");
    }

    public override void PhysicsUpdate() { }
    public override void OnExit() { }
}