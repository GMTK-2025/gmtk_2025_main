using UnityEngine;

public class PlayerDeadState : PlayerState
{
    public override void OnEnter(PlayerController player)
    {
        this._player = player;

        // ����������ƣ������ƶ�����Ծ��
        player.inputControl.Player.Move.Disable();
        player.inputControl.Player.Jump.Disable();

        // ֹͣ�����˶�
        player.rb.linearVelocity = Vector2.zero;
        player.rb.simulated = false;

        // ����������Ч�����β��ţ�
        player.PlaySound(player.dieSound);

        Debug.Log("Player entered dead state");
    }

    public override void LogicUpdate()
    {
        // ����״̬�����߼����£���������Ϸ�����߼�
    }

    public override void PhysicsUpdate()
    {
        // ����״̬������������
    }

    public override void OnExit(PlayerController player)
    {
        // �˳�����״̬ʱ�ָ�����
        player.inputControl.Player.Move.Enable();
        player.inputControl.Player.Jump.Enable();
        player.rb.simulated = true;
    }
}