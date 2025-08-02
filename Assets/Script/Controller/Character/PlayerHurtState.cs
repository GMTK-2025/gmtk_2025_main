using UnityEngine;

public class PlayerHurtState : PlayerState
{
    // ����״̬����ʱ��
    private float hurtDuration = 0.2f;
    private float hurtTimer;

    // ���˲���
    private Vector2 knockbackForce = new Vector2(-3f, 5f);

    public override void OnEnter(PlayerController player)
    {
        base.OnEnter(player);
        Debug.Log("��������״̬");

        // 1. ��Ѫ�߼�
        GhostSystem ghostSystem = player.GetGhostSystem();
        if (ghostSystem != null && ghostSystem.currentLives > 0)
        {
            ghostSystem.currentLives--;
            Debug.Log($"����ֵ����ʣ��: {ghostSystem.currentLives}/{ghostSystem.maxLives}");
        }
        else
        {
            Debug.LogWarning("�޷���Ѫ��GhostSystemδ�ҵ�������ֵ����");
        }


        player.isInvincible = true;
        player.invincibleTimer = player.globalInvincibleDuration;
        Debug.Log($"�����޵�״̬������ {player.globalInvincibleDuration} ��");


   

        hurtTimer = hurtDuration;
    }

    public override void LogicUpdate()
    {
        if (player == null) return;

        hurtTimer -= Time.deltaTime;
        if (hurtTimer <= 0)
        {
            // �л�������״̬
            if (player.physicsCheck.isGround)
            {
                player.SwitchState(player.runState);
            }
            else
            {
                player.SwitchState(player.fallState);
            }
        }
    }

    public override void PhysicsUpdate() { }

    public override void OnExit(PlayerController player) { }

    // �޵�״̬�ж�
    public override bool IsInvincible() => player.isInvincible;
}
