using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthPickup : MonoBehaviour
{
    [Header("恢复设置")]
    [Tooltip("每次拾取恢复的生命值")]
    public int healthRestoreAmount = 1;

    [Tooltip("拾取后是否销毁物体")]
    public bool destroyOnPickup = true;

    [Tooltip("物体销毁延迟时间（秒），0=立即销毁")]
    public float destroyDelay = 0f; // 新增：自定义销毁延迟

    [Header("音频设置")]
    [Tooltip("拾取时播放的音效")]
    public AudioClip pickupSound;

    [Range(0, 1)]
    public float soundVolume = 1f; // 音效音量

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                GhostSystem ghostSystem = player.GetGhostSystem();
                if (ghostSystem != null)
                {
                    RestoreHealth(ghostSystem);
                }
            }
        }
    }

    private void RestoreHealth(GhostSystem ghostSystem)
    {
        if (ghostSystem.currentLives < ghostSystem.maxLives)
        {
            // 恢复生命值
            ghostSystem.currentLives = Mathf.Min(
                ghostSystem.currentLives + healthRestoreAmount,
                ghostSystem.maxLives
            );

           
            PlayPickupSound();

           
            HandleDestroy();
        }
    }

    
    private void PlayPickupSound()
    {
        if (pickupSound == null) return;

        // 创建临时游戏对象用于播放音效
        GameObject soundPlayer = new GameObject("PickupSoundPlayer");
        // 将临时对象放在拾取物位置
        soundPlayer.transform.position = transform.position;

        // 添加音频源组件
        AudioSource tempAudio = soundPlayer.AddComponent<AudioSource>();
        tempAudio.clip = pickupSound;
        tempAudio.volume = soundVolume;
        tempAudio.spatialBlend = 0;
        tempAudio.Play(); 

        // 音效播放完毕后自动销毁临时对象
        Destroy(soundPlayer, pickupSound.length);
    }

    // 处理物体销毁：按自定义延迟时间执行
    private void HandleDestroy()
    {
        if (!destroyOnPickup) return;

        // 立即禁用碰撞体和渲染
        GetComponent<Collider2D>().enabled = false;
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;

        // 延迟销毁物体
        Destroy(gameObject, destroyDelay);
    }
}