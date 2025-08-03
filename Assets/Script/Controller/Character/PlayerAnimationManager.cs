using UnityEngine;

/// <summary>
/// ��ɫ������������ͳһ�������ж�����������
/// </summary>
public class PlayerAnimationManager : MonoBehaviour
{
    // �����������
    private Animator _anim;

    // �����������������й�������ħ���ַ�����
    public static class AnimParams
    {
        public const string IsWalking = "IsWalking";   // ��·״̬
        public const string IsJumping = "IsJumping";   // ��Ծ״̬
        public const string IsFalling = "IsFalling";   // ����״̬
        public const string IsHurt = "IsHurt";         // ����״̬
        public const string IsClimbing = "IsClimbing"; // ����״̬
        public const string IsDead = "IsDead";         // ����״̬
        public const string Speed = "Speed";           // �ƶ��ٶȣ���ѡ�����ھ�ϸ���ƣ�
    }

    private void Awake()
    {
        // ��ȡAnimator���
        _anim = GetComponent<Animator>();
        if (_anim == null)
        {
            Debug.LogError("PlayerAnimationManager�Ҳ���Animator�������ȷ����ɫ�����������Animator��");
        }
    }

    // ------------------- �����������Ʒ��� -------------------

    /// <summary>
    /// ������·����״̬
    /// </summary>
    /// <param name="isWalking">�Ƿ�������·</param>
    public void SetWalking(bool isWalking)
    {
        if (_anim != null)
        {
            _anim.SetBool(AnimParams.IsWalking, isWalking);
        }
    }

    /// <summary>
    /// ������Ծ������һ���Դ�����
    /// </summary>
    public void TriggerJump()
    {
        if (_anim != null)
        {
            _anim.SetTrigger(AnimParams.IsJumping);
        }
    }

    /// <summary>
    /// �������䶯��״̬
    /// </summary>
    /// <param name="isFalling">�Ƿ���������</param>
    public void SetFalling(bool isFalling)
    {
        if (_anim != null)
        {
            _anim.SetBool(AnimParams.IsFalling, isFalling);
        }
    }

    /// <summary>
    /// �������˶���
    /// </summary>
    public void TriggerHurt()
    {
        if (_anim != null)
        {
            _anim.SetTrigger(AnimParams.IsHurt);
        }
    }

    /// <summary>
    /// ������������״̬
    /// </summary>
    /// <param name="isClimbing">�Ƿ���������</param>
    public void SetClimbing(bool isClimbing)
    {
        if (_anim != null)
        {
            _anim.SetBool(AnimParams.IsClimbing, isClimbing);
        }
    }

    /// <summary>
    /// ������������״̬
    /// </summary>
    /// <param name="isDead">�Ƿ�����</param>
    public void SetDead(bool isDead)
    {
        if (_anim != null)
        {
            _anim.SetBool(AnimParams.IsDead, isDead);
        }
    }

    /// <summary>
    /// �����ƶ��ٶȲ��������ڶ������ɾ�ϸ���ƣ�
    /// </summary>
    /// <param name="speed">�ƶ��ٶȣ�����ֵ��</param>
    public void SetSpeed(float speed)
    {
        if (_anim != null)
        {
            _anim.SetFloat(AnimParams.Speed, speed);
        }
    }

    /// <summary>
    /// �������ж���������״̬�л�ʱ���ã�
    /// </summary>
    public void ResetAllParams()
    {
        if (_anim != null)
        {
            _anim.SetBool(AnimParams.IsWalking, false);
            _anim.SetBool(AnimParams.IsFalling, false);
            _anim.SetBool(AnimParams.IsClimbing, false);
            _anim.SetBool(AnimParams.IsDead, false);
            _anim.SetFloat(AnimParams.Speed, 0);
        }
    }
}