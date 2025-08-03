using UnityEngine;

public class PlayerHurtState : PlayerState
{
    // 受伤状态持续时间
    private float hurtDuration = 0.2f;
    private float hurtTimer;

    // 击退参数
    private Vector2 knockbackForce = new Vector2(-3f, 5f);

    // 记录伤害源
    private GameObject damageSource;

    public override void OnEnter(PlayerController player)
    {
        base.OnEnter(player);
        Debug.Log($"进入受伤状态，伤害源: {damageSource?.name ?? "未知对象"}");
        player.PlaySound(player.hurtSound); // 播放受伤音效

        // 扣血逻辑
        GhostSystem ghostSystem = player.GetGhostSystem();
        if (ghostSystem != null && ghostSystem.currentLives > 0)
        {
            ghostSystem.currentLives--;
            Debug.Log($"生命值减少，剩余: {ghostSystem.currentLives}/{ghostSystem.maxLives}，伤害来自: {damageSource?.name}");
        }
        else
        {
            Debug.LogWarning($"无法扣血：GhostSystem未找到或生命值不足，伤害来自: {damageSource?.name}");
        }

        player.isInvincible = true;
        player.invincibleTimer = player.globalInvincibleDuration;
        Debug.Log($"启动无敌状态，持续 {player.globalInvincibleDuration} 秒");

        hurtTimer = hurtDuration;
    }

    // 新增方法：设置伤害源
    public void SetDamageSource(GameObject source)
    {
        damageSource = source;
    }

    public override void LogicUpdate()
    {
        if (player == null) return;

        hurtTimer -= Time.deltaTime;
        if (hurtTimer <= 0)
        {
            // 切换到正常状态
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

    public override void OnExit(PlayerController player)
    {
        // 清除伤害源引用
        damageSource = null;
    }

    // 无敌状态判断
    public override bool IsInvincible() => player.isInvincible;
}
