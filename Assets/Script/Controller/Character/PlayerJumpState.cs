using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public override void OnEnter(PlayerController player)
    {
        this.player = player;
        // ʩ����Ծ�������ڵ�����ʱ��
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
        // ��غ��л����ܲ�״̬
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
