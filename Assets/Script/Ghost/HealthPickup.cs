using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthPickup : MonoBehaviour
{
    [Header("�ָ�����")]
    [Tooltip("ÿ��ʰȡ�ָ�������ֵ")]
    public int healthRestoreAmount = 1;

    [Tooltip("ʰȡ���Ƿ���������")]
    public bool destroyOnPickup = true;

    [Tooltip("���������ӳ�ʱ�䣨�룩��0=��������")]
    public float destroyDelay = 0f; // �������Զ��������ӳ�

    [Header("��Ƶ����")]
    [Tooltip("ʰȡʱ���ŵ���Ч")]
    public AudioClip pickupSound;

    [Range(0, 1)]
    public float soundVolume = 1f; // ��Ч����

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
            // �ָ�����ֵ
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

        // ������ʱ��Ϸ�������ڲ�����Ч
        GameObject soundPlayer = new GameObject("PickupSoundPlayer");
        // ����ʱ�������ʰȡ��λ��
        soundPlayer.transform.position = transform.position;

        // �����ƵԴ���
        AudioSource tempAudio = soundPlayer.AddComponent<AudioSource>();
        tempAudio.clip = pickupSound;
        tempAudio.volume = soundVolume;
        tempAudio.spatialBlend = 0;
        tempAudio.Play(); 

        // ��Ч������Ϻ��Զ�������ʱ����
        Destroy(soundPlayer, pickupSound.length);
    }

    // �����������٣����Զ����ӳ�ʱ��ִ��
    private void HandleDestroy()
    {
        if (!destroyOnPickup) return;

        // ����������ײ�����Ⱦ
        GetComponent<Collider2D>().enabled = false;
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;

        // �ӳ���������
        Destroy(gameObject, destroyDelay);
    }
}