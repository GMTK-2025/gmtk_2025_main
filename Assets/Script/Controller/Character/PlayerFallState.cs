using UnityEngine;

public class PlayerFallState : PlayerState
{
    public override void OnEnter(PlayerController player)
    {
        this.player = player;
   
        // player.animator.SetTrigger("Fall");
    }

    public override void LogicUpdate()
    {
      
        if (player.physicsCheck.isGround)
        {
            player.SwitchState(player.runState);
        }
      
        else if (player.inputControl.Player.Jump.WasPressedThisFrame() &&
                player.currentJumpCount < player.maxJumpCount)
        {
            player.SwitchState(new PlayerJumpState());
        }
    }

    public override void PhysicsUpdate()
    {
       
        player.rb.linearVelocity = new Vector2(
            player.inputDirection.x * player.speed,
            player.rb.linearVelocity.y
        );
    }

    public override void OnExit() { }
}