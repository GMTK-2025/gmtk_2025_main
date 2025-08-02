using UnityEngine;

/// <summary>
/// 玩家胜利状态（进入游戏结束区域等逻辑）
/// </summary>
public class PlayerWinState : PlayerState
{
    public override void OnEnter(PlayerController player)
    {
        base.OnEnter(player);
        Debug.Log($"[{player.name}] 进入胜利状态");
     
        player.PlaySound(player.winSound);
    }

    public override void LogicUpdate()
    {
      
    
    }

    public override void PhysicsUpdate()
    {
       
    }

    public override void OnExit(PlayerController player)
    {
        
       
    }
}