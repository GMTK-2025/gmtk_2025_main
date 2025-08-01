using UnityEngine;

public class PlayerRunState : PlayerState
{
    public override void OnEnter(PlayerController player)
    {
        this.player = player;
    }

    public override void LogicUpdate()
    {
        // 检测跳跃输入，支持二段跳
        if (player.inputControl.Player.Jump.WasPressedThisFrame())
        {
            // 允许跳跃的条件：要么在地面上，要么在空中但未达到最大跳跃次数
            if (player.physicsCheck.isGround || player.currentJumpCount < player.maxJumpCount - 1)
            {
                player.SwitchState(player.jumpState);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        // 设置水平移动速度
        player.rb.linearVelocity = new Vector2(
            player.inputDirection.x * player.speed,
            player.rb.linearVelocity.y
        );

        if (player.inputDirection.x != 0)
        {
            int faceDir = player.inputDirection.x > 0 ? 1 : -1;
            player.transform.localScale = new Vector3(faceDir, 1, 1);

            // 处理子对象翻转
            foreach (Transform child in player.transform)
            {
                child.localScale = new Vector3(1.0f / faceDir, 1, 1);
            }
        }
    }

    public override void OnExit()
    {

    }
}
