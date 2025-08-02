using UnityEngine;

public class PlayerHurtState : PlayerState
{
    // 受伤状态持续时间
    private float hurtDuration = 0.2f;
    private float hurtTimer;

    // 击退参数
    private Vector2 knockbackForce = new Vector2(-3f, 5f);

    public override void OnEnter(PlayerController player)
    {
        base.OnEnter(player);
        Debug.Log("进入受伤状态");

        // 1. 扣血逻辑
        GhostSystem ghostSystem = player.GetGhostSystem();
        if (ghostSystem != null && ghostSystem.currentLives > 0)
        {
            ghostSystem.currentLives--;
            Debug.Log($"生命值减少剩余: {ghostSystem.currentLives}/{ghostSystem.maxLives}");
        }
        else
        {
            Debug.LogWarning("无法扣血：GhostSystem未找到或生命值不足");
        }


        player.isInvincible = true;
        player.invincibleTimer = player.globalInvincibleDuration;
        Debug.Log($"启动无敌状态，持续 {player.globalInvincibleDuration} 秒");


   

        hurtTimer = hurtDuration;
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

    public override void OnExit(PlayerController player) { }

    // 无敌状态判断
    public override bool IsInvincible() => player.isInvincible;
}
