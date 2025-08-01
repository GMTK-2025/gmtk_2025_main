using UnityEngine;

public class PlayerDeadState : PlayerState
{
    public override void OnEnter(PlayerController player)
    {
        this.player = player;


        player.inputControl.Player.Move.Disable();


        player.rb.linearVelocity = Vector2.zero;
        player.rb.simulated = false;


        Debug.Log("Player entered dead state");
    }

    public override void LogicUpdate()
    {
    }

    public override void PhysicsUpdate()
    {
    }

    public override void OnExit(PlayerController player)
    {
        player.inputControl.Player.Move.Enable();
        player.rb.simulated = true;
    }
}