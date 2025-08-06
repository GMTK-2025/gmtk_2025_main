using UnityEngine;

public class PlayerFallState : PlayerState
{
    public override void OnEnter(PlayerController player)
    {
        this._player = player;
   
        // player.animator.SetTrigger("Fall");
    }

    public override void LogicUpdate()
    {
      
        if (_player.physicsCheck.isGround)
        {
            _player.SwitchState(_player.runState);
        }
      
        else if (_player.inputControl.Player.Jump.WasPressedThisFrame() &&
                _player.currentJumpCount < _player.maxJumpCount)
        {
            _player.SwitchState(new PlayerJumpState());
        }
    }

    public override void PhysicsUpdate()
    {
       
        _player.rb.linearVelocity = new Vector2(
            _player.inputDirection.x * _player.speed,
            _player.rb.linearVelocity.y
        );
    }

    public override void OnExit(PlayerController player) { }
}