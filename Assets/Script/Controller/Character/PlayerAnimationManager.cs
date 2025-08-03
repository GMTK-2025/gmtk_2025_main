using UnityEngine;

/// <summary>
/// 角色动画管理器：统一处理所有动画参数控制
/// </summary>
public class PlayerAnimationManager : MonoBehaviour
{
    // 动画组件引用
    private Animator _anim;

    // 动画参数常量（集中管理，避免魔法字符串）
    public static class AnimParams
    {
        public const string IsWalking = "IsWalking";   // 走路状态
        public const string IsJumping = "IsJumping";   // 跳跃状态
        public const string IsFalling = "IsFalling";   // 下落状态
        public const string IsHurt = "IsHurt";         // 受伤状态
        public const string IsClimbing = "IsClimbing"; // 攀爬状态
        public const string IsDead = "IsDead";         // 死亡状态
        public const string Speed = "Speed";           // 移动速度（可选，用于精细控制）
    }

    private void Awake()
    {
        // 获取Animator组件
        _anim = GetComponent<Animator>();
        if (_anim == null)
        {
            Debug.LogError("PlayerAnimationManager找不到Animator组件！请确保角色对象上已添加Animator。");
        }
    }

    // ------------------- 基础动画控制方法 -------------------

    /// <summary>
    /// 设置走路动画状态
    /// </summary>
    /// <param name="isWalking">是否正在走路</param>
    public void SetWalking(bool isWalking)
    {
        if (_anim != null)
        {
            _anim.SetBool(AnimParams.IsWalking, isWalking);
        }
    }

    /// <summary>
    /// 触发跳跃动画（一次性触发）
    /// </summary>
    public void TriggerJump()
    {
        if (_anim != null)
        {
            _anim.SetTrigger(AnimParams.IsJumping);
        }
    }

    /// <summary>
    /// 设置下落动画状态
    /// </summary>
    /// <param name="isFalling">是否正在下落</param>
    public void SetFalling(bool isFalling)
    {
        if (_anim != null)
        {
            _anim.SetBool(AnimParams.IsFalling, isFalling);
        }
    }

    /// <summary>
    /// 触发受伤动画
    /// </summary>
    public void TriggerHurt()
    {
        if (_anim != null)
        {
            _anim.SetTrigger(AnimParams.IsHurt);
        }
    }

    /// <summary>
    /// 设置攀爬动画状态
    /// </summary>
    /// <param name="isClimbing">是否正在攀爬</param>
    public void SetClimbing(bool isClimbing)
    {
        if (_anim != null)
        {
            _anim.SetBool(AnimParams.IsClimbing, isClimbing);
        }
    }

    /// <summary>
    /// 设置死亡动画状态
    /// </summary>
    /// <param name="isDead">是否死亡</param>
    public void SetDead(bool isDead)
    {
        if (_anim != null)
        {
            _anim.SetBool(AnimParams.IsDead, isDead);
        }
    }

    /// <summary>
    /// 设置移动速度参数（用于动画过渡精细控制）
    /// </summary>
    /// <param name="speed">移动速度（绝对值）</param>
    public void SetSpeed(float speed)
    {
        if (_anim != null)
        {
            _anim.SetFloat(AnimParams.Speed, speed);
        }
    }

    /// <summary>
    /// 重置所有动画参数（状态切换时调用）
    /// </summary>
    public void ResetAllParams()
    {
        if (_anim != null)
        {
            _anim.SetBool(AnimParams.IsWalking, false);
            _anim.SetBool(AnimParams.IsFalling, false);
            _anim.SetBool(AnimParams.IsClimbing, false);
            _anim.SetBool(AnimParams.IsDead, false);
            _anim.SetFloat(AnimParams.Speed, 0);
        }
    }
}