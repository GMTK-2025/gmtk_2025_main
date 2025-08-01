using UnityEngine;

public class PlatformDetector : MonoBehaviour
{
    public ButtonPlatform buttonPlatform;

    // 确保碰撞器配置正确
    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
           
        }
        else if (!col.isTrigger)
        {
          
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // 忽略自身或不需要检测的物体
        if (other.gameObject == gameObject || other.CompareTag(buttonPlatform.ignoreTag))
            return;

        if (buttonPlatform != null)
        {
            buttonPlatform.OnPlatformEnter(other);
        }
      
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null) return;

        if (buttonPlatform != null)
        {
            buttonPlatform.OnPlatformExit(other);
        }
    }
}