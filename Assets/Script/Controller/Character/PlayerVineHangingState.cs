using UnityEngine;
using System.Collections;

public class PlayerVineHangingState : PlayerState
{
    private float hangTimer;
    private float originalLinearDamping;
    private float originalGravity;
    private bool isExiting = false;
    private bool isTouchingVine = false; 

    public override void OnEnter(PlayerController player)
    {
        this.player = player;
        hangTimer = player.vineHangDuration;
        isExiting = false;
        isTouchingVine = true; 

        // 保存原始物理参数
        originalLinearDamping = player.rb.linearDamping;
        originalGravity = player.rb.gravityScale;

        // 应用藤蔓缓降参数
        player.rb.linearDamping = player.vineLinearDrag;
        player.rb.gravityScale = 1f;
        player.rb.linearVelocity = Vector2.zero; // 清零初始速度
    }

    public override void LogicUpdate()
    {
        if (isExiting) return;

        // 离开藤蔓时立即退出状态
        if (!isTouchingVine)
        {
            ExitVineHang();
            return;
        }

        // 计时退出
        hangTimer -= Time.deltaTime;
        if (hangTimer <= 0 || (player.physicsCheck.isGround && player.rb.linearVelocity.y <= 0.1f))
        {
            ExitVineHang();
        }
    }

    public override void PhysicsUpdate()
    {
        if (isExiting || !isTouchingVine) return;

        // 接触藤蔓时的缓慢下落
        if (player.rb.linearVelocity.y < -player.vineFallSpeed)
        {
            player.rb.linearVelocity = new Vector2(
                player.rb.linearVelocity.x * 0.9f, 
                -player.vineFallSpeed              // 限制最大下落速度
            );
        }
    }

  
    public void UpdateVineContact(bool isTouching)
    {
        isTouchingVine = isTouching;
    }

    private void ExitVineHang()
    {
        if (isExiting) return;
        isExiting = true;

        // 清零速度，避免残留运动
        player.rb.linearVelocity = Vector2.zero;

        // 恢复原始物理参数（立即恢复，确保离开后正常下落）
        player.rb.linearDamping = originalLinearDamping;
        player.rb.gravityScale = originalGravity;

        // 延迟一帧切换状态，确保物理参数生效
        player.StartCustomCoroutine(SwitchStateAfterFrame());
    }

    private IEnumerator SwitchStateAfterFrame()
    {
        yield return new WaitForFixedUpdate();

        // 根据落地状态切换到对应状态
        if (player.physicsCheck.isGround)
        {
            player.SwitchState(player.runState);
        }
        else
        {
            player.SwitchState(player.fallState); // 离开藤蔓后进入正常下落状态
        }
    }

    public override void OnExit()
    {
        isExiting = true;
        // 强制恢复物理参数
        player.rb.linearDamping = originalLinearDamping;
        player.rb.gravityScale = originalGravity;
        player.rb.linearVelocity = Vector2.zero;
    }
}