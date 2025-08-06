using Event;
using UnityEngine;

public class PlayerHurtState : PlayerState
{
    // ����״̬����ʱ��
    private float hurtDuration = 0.2f;
    private float hurtTimer;

    // ���˲���
    private Vector2 knockbackForce = new Vector2(-3f, 5f);

    // ��¼�˺�Դ
    private GameObject damageSource;

    public override void OnEnter(PlayerController player)
    {
        base.OnEnter(player);
        Debug.Log($"��������״̬���˺�Դ: {damageSource?.name ?? "δ֪����"}");
        player.PlaySound(player.hurtSound, player.hurtSoundVolume);

        // ��Ѫ�߼�
        GhostSystem ghostSystem = player.GetGhostSystem();
        if (ghostSystem != null && ghostSystem.currentLives > 0)
        {
            ghostSystem.currentLives--;
            EventBus.Character.SendMessage(CharacterEventType.CHARACTER_HURT);
            Debug.Log($"����ֵ���٣�ʣ��: {ghostSystem.currentLives}/{ghostSystem.maxLives}���˺�����: {damageSource?.name}");
        }
        else
        {
            Debug.LogWarning($"�޷���Ѫ��GhostSystemδ�ҵ�������ֵ���㣬�˺�����: {damageSource?.name}");
        }

        player.isInvincible = true;
        player.invincibleTimer = player.globalInvincibleDuration;
        Debug.Log($"�����޵�״̬������ {player.globalInvincibleDuration} ��");

        hurtTimer = hurtDuration;
    }

    // ���������������˺�Դ
    public void SetDamageSource(GameObject source)
    {
        damageSource = source;
    }

    public override void LogicUpdate()
    {
        if (_player == null) return;

        hurtTimer -= Time.deltaTime;
        if (hurtTimer <= 0)
        {
            // �л�������״̬
            if (_player.physicsCheck.isGround)
            {
                _player.SwitchState(_player.runState);
            }
            else
            {
                _player.SwitchState(_player.fallState);
            }
        }
    }

    public override void PhysicsUpdate() { }

    public override void OnExit(PlayerController player)
    {
        // ����˺�Դ����
        damageSource = null;
    }

    // �޵�״̬�ж�
    public override bool IsInvincible() => _player.isInvincible;
}
