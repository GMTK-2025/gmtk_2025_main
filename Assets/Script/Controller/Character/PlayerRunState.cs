using UnityEngine;

public class PlayerRunState : PlayerState
{
    // 跑步音效控制变量
    private bool isPlayingRunSound = false; // 当前是否在播放跑步音效
    private bool isRunning = false; // 当前帧是否在移动

    public override void OnEnter(PlayerController player)
    {
        this.player = player;
        // 初始化音效状态
        isPlayingRunSound = false;
        isRunning = false;
    }

    public override void LogicUpdate()
    {
        player.animManager.SetWalking(isRunning);
        // 检测跳跃输入，支持二段跳
        if (player.inputControl.Player.Jump.WasPressedThisFrame())
        {
            // 允许跳跃的条件：要么在地面上，要么在空中但未达到最大跳跃次数
            if (player.physicsCheck.isGround || player.currentJumpCount < player.maxJumpCount - 1)
            {
                player.SwitchState(player.jumpState);
                return; // 切换状态后退出当前帧逻辑
            }
        }

        // 更新跑步音效状态（根据移动状态控制）
        UpdateRunSound();
    }

    public override void PhysicsUpdate()
    {
        // 设置水平移动速度
        player.rb.linearVelocity = new Vector2(
            player.inputDirection.x * player.speed,
            player.rb.linearVelocity.y
        );

        // 判断是否正在移动（左右方向键按下）
        isRunning = player.inputDirection.x != 0;

        if (isRunning)
        {
            // 角色翻转逻辑
            int faceDir = player.inputDirection.x > 0 ? 1 : -1;
            player.transform.localScale = new Vector3(faceDir, 1, 1);

            // 处理子对象翻转
            //foreach (Transform child in player.transform)
            //{
            //    child.localScale = new Vector3(1.0f / faceDir, 1, 1);
            //}
        }
    }

    // 跑步音效控制核心逻辑
    private void UpdateRunSound()
    {
        // 仅在地面上时播放跑步音效（避免空中移动误触发）
        if (!player.physicsCheck.isGround)
        {
            // 不在地面时强制停止音效
            if (isPlayingRunSound)
            {
                player.StopLoopSound();
                isPlayingRunSound = false;
            }
            return;
        }

        // 移动时播放循环音效
        if (isRunning && !isPlayingRunSound)
        {
            player.PlayLoopSound(player.runSound); // 播放跑步循环音效
            isPlayingRunSound = true;
        }
        // 停止移动时停止音效
        else if (!isRunning && isPlayingRunSound)
        {
            player.StopLoopSound(); // 停止跑步音效
            isPlayingRunSound = false;
        }
    }

    public override void OnExit(PlayerController player)
    {
        // 退出跑步状态时强制停止音效
        if (isPlayingRunSound)
        {
            player.StopLoopSound();
            isPlayingRunSound = false;
        }

        player.animManager.SetWalking(false);
    }
}