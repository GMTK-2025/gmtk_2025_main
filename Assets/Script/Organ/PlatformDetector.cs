using UnityEngine;

public class PlatformDetector : MonoBehaviour
{
    public ButtonPlatform buttonPlatform;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (buttonPlatform != null)
        {
            buttonPlatform.OnPlatformEnter(other);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (buttonPlatform != null)
        {
            buttonPlatform.OnPlatformExit(other);
        }
    }
}
