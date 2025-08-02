using UnityEngine;

/// <summary>
/// 玩家状态基类，所有具体状态需继承并实现抽象方法
/// </summary>
public abstract class PlayerState
{
    protected PlayerController player;

    /// <summary>
    /// 进入状态时调用（初始化逻辑）
    /// </summary>
    /// <param name="player">玩家控制器实例</param>
    public virtual void OnEnter(PlayerController player)
    {
        this.player = player;
    }

    /// <summary>
    /// 逻辑更新（每帧调用，处理输入、状态切换等）
    /// </summary>
    public abstract void LogicUpdate();

    /// <summary>
    /// 物理更新（固定帧率调用，处理运动、碰撞等物理逻辑）
    /// </summary>
    public abstract void PhysicsUpdate();

    /// <summary>
    /// 退出状态时调用（清理逻辑）
    /// </summary>
    /// <param name="player">玩家控制器实例</param>
    public abstract void OnExit(PlayerController player);

    /// <summary>
    /// 判断当前状态是否无敌（可被子类重写）
    /// </summary>
    /// <returns>true=无敌，false=可受伤</returns>
    public virtual bool IsInvincible()
    {
        return player != null && player.isInvincible;
    }

    /// <summary>
    /// 安全切换状态（封装空引用校验）
    /// </summary>
    /// <param name="newState">目标状态</param>
    protected void SwitchState(PlayerState newState)
    {
        if (newState == null || player == null)
        {
            Debug.LogError($"[{GetType().Name}] 切换状态失败：状态或玩家控制器为空！", player?.gameObject);
            return;
        }
        player.SwitchState(newState);
    }
}