using UnityEngine;

/// <summary>
/// ���״̬���࣬���о���״̬��̳в�ʵ�ֳ��󷽷�
/// </summary>
public abstract class PlayerState
{
    protected PlayerController player;

    /// <summary>
    /// ����״̬ʱ���ã���ʼ���߼���
    /// </summary>
    /// <param name="player">��ҿ�����ʵ��</param>
    public virtual void OnEnter(PlayerController player)
    {
        this.player = player;
    }

    /// <summary>
    /// �߼����£�ÿ֡���ã��������롢״̬�л��ȣ�
    /// </summary>
    public abstract void LogicUpdate();

    /// <summary>
    /// �������£��̶�֡�ʵ��ã������˶�����ײ�������߼���
    /// </summary>
    public abstract void PhysicsUpdate();

    /// <summary>
    /// �˳�״̬ʱ���ã������߼���
    /// </summary>
    /// <param name="player">��ҿ�����ʵ��</param>
    public abstract void OnExit(PlayerController player);

    /// <summary>
    /// �жϵ�ǰ״̬�Ƿ��޵У��ɱ�������д��
    /// </summary>
    /// <returns>true=�޵У�false=������</returns>
    public virtual bool IsInvincible()
    {
        return player != null && player.isInvincible;
    }

    /// <summary>
    /// ��ȫ�л�״̬����װ������У�飩
    /// </summary>
    /// <param name="newState">Ŀ��״̬</param>
    protected void SwitchState(PlayerState newState)
    {
        if (newState == null || player == null)
        {
            Debug.LogError($"[{GetType().Name}] �л�״̬ʧ�ܣ�״̬����ҿ�����Ϊ�գ�", player?.gameObject);
            return;
        }
        player.SwitchState(newState);
    }
}