using UnityEngine;

public class PlayerRunState : PlayerState
{
    public override void OnEnter(PlayerController player)
    {
        this.player = player;
    }

    public override void LogicUpdate()
    {
        // �����Ծ���룬�л�����Ծ״̬
        if (player.inputControl.Player.Jump.WasPressedThisFrame())
        {
            player.SwitchState(player.jumpState);
        }
    }

    public override void PhysicsUpdate()
    {
        // ����ˮƽ�ƶ��ٶ�
        player.rb.linearVelocity = new Vector2(player.inputDirection.x * player.speed,player.rb.linearVelocity.y);

        if (player.inputDirection.x != 0)
        {
            int faceDir = player.inputDirection.x > 0 ? 1 : -1;
            player.transform.localScale = new Vector3(faceDir, 1, 1);

            // �����Ӷ���ת
            foreach (Transform child in player.transform)
            {
                child.localScale = new Vector3(1.0f / faceDir, 2, 1);
            }
        }
    }

    public override void OnExit()
    {
     
    }
}
