using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthPickup : MonoBehaviour
{
    [Header("�ָ�����")]
    [Tooltip("ÿ��ʰȡ�ָ�������ֵ")]
    public int healthRestoreAmount = 1;

    [Tooltip("ʰȡ���Ƿ���������")]
    public bool destroyOnPickup = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // �����ײ�����Ƿ�Ϊ���
        if (other.CompareTag("Player"))
        {
            // ��ȡ��ҿ�����
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // ͨ����ҿ�������ȡGhostSystem������ֵ���������
                GhostSystem ghostSystem = player.GetGhostSystem();
                if (ghostSystem != null)
                {
                    // ���Իָ�����ֵ
                    RestoreHealth(ghostSystem);
                }
            }
        }
    }

    private void RestoreHealth(GhostSystem ghostSystem)
    {
        // ����Ƿ���Իָ�����ֵ��δ�ﵽ���ֵ��
        if (ghostSystem.currentLives < ghostSystem.maxLives)
        {
            // ��������ֵ�����������ֵ
            ghostSystem.currentLives = Mathf.Min(
                ghostSystem.currentLives + healthRestoreAmount,
                ghostSystem.maxLives
            );

            // ����ʰȡ��
            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}
