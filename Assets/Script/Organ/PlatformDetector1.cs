using UnityEngine;

public class PlatformDetector1 : MonoBehaviour
{
    public ButtonPlatform1 buttonPlatform;

    // 物体进入平台触发区
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null)
        {
            buttonPlatform?.OnPlatformEnter(other);
        }
    }

    // 物体离开平台触发区
    void OnTriggerExit2D(Collider2D other)
    {
        if (other != null)
        {
            buttonPlatform?.OnPlatformExit(other);
        }
    }

    // 绘制检测范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            if (col is BoxCollider2D box)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
            }
            else if (col is CircleCollider2D circle)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
        }
    }
}