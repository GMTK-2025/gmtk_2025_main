using UnityEngine;

public abstract class PlayerState
{
    protected PlayerController player;

    // ������ʼ�����洢�������
    public virtual void OnEnter(PlayerController player)
    {
        this.player = player;
    }

    public abstract void LogicUpdate();
    public abstract void PhysicsUpdate();
    public abstract void OnExit(PlayerController player);

    // �޵�״̬�жϣ�Ĭ�Ϸ���ȫ���޵�״̬��
    public virtual bool IsInvincible() => player.isInvincible;
}