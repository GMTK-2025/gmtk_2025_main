using UnityEngine;

public abstract class PlayerState
{
    protected PlayerController player;

    // 基础初始化：存储玩家引用
    public virtual void OnEnter(PlayerController player)
    {
        this.player = player;
    }

    public abstract void LogicUpdate();
    public abstract void PhysicsUpdate();
    public abstract void OnExit(PlayerController player);

    // 无敌状态判断（默认返回全局无敌状态）
    public virtual bool IsInvincible() => player.isInvincible;
}