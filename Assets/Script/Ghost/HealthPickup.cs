using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthPickup : MonoBehaviour
{
    [Header("恢复设置")]
    [Tooltip("每次拾取恢复的生命值")]
    public int healthRestoreAmount = 1;

    [Tooltip("拾取后是否销毁物体")]
    public bool destroyOnPickup = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测碰撞对象是否为玩家
        if (other.CompareTag("Player"))
        {
            // 获取玩家控制器
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // 通过玩家控制器获取GhostSystem（生命值管理在这里）
                GhostSystem ghostSystem = player.GetGhostSystem();
                if (ghostSystem != null)
                {
                    // 尝试恢复生命值
                    RestoreHealth(ghostSystem);
                }
            }
        }
    }

    private void RestoreHealth(GhostSystem ghostSystem)
    {
        // 检查是否可以恢复生命值（未达到最大值）
        if (ghostSystem.currentLives < ghostSystem.maxLives)
        {
            // 增加生命值，不超过最大值
            ghostSystem.currentLives = Mathf.Min(
                ghostSystem.currentLives + healthRestoreAmount,
                ghostSystem.maxLives
            );

            // 销毁拾取物
            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}
