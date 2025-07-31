using System.Collections.Generic;
using UnityEngine;

public class ButtonPlatform : MonoBehaviour
{
    [Header("平台设置")]
    public Transform platform;              // 平台物体
    public float platformMoveDistance = 5f; // 平台移动距离
    public float buttonPressDepth = 0.2f;   // 按钮按下深度
    public float moveSpeed = 5f;            // 移动速度

    private Vector3 originalPlatformPosition;
    private Vector3 originalButtonPosition;
    private HashSet<Collider2D> pressingObjects = new HashSet<Collider2D>();
    private HashSet<Collider2D> platformObjects = new HashSet<Collider2D>();
    private bool isPressed;
    private Vector3 lastPlatformPosition;

    void Start()
    {
        originalPlatformPosition = platform.position;
        originalButtonPosition = transform.position;
        lastPlatformPosition = originalPlatformPosition;
    }

    void Update()
    {
        bool currentPressed = pressingObjects.Count > 0;

        if (currentPressed && platformObjects.Count > 0)
        {
            currentPressed = false;
        }

        isPressed = currentPressed;

        Vector3 targetPlatformPos = isPressed ?
            originalPlatformPosition + Vector3.left * platformMoveDistance :
            originalPlatformPosition;

        platform.position = Vector3.Lerp(platform.position, targetPlatformPos, moveSpeed * Time.deltaTime);

        Vector3 platformDelta = platform.position - lastPlatformPosition;
        lastPlatformPosition = platform.position;

        MovePlatformObjects(platformDelta);

        Vector3 targetButtonPos = isPressed ?
            originalButtonPosition + Vector3.down * buttonPressDepth :
            originalButtonPosition;
        transform.position = Vector3.Lerp(transform.position, targetButtonPos, moveSpeed * Time.deltaTime);
    }

    private void MovePlatformObjects(Vector3 platformDelta)
    {
        if (platformDelta.sqrMagnitude < 0.0001f) return;

        foreach (var obj in platformObjects)
        {
            if (obj == null) continue;

            if (obj.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                if (rb.bodyType == RigidbodyType2D.Dynamic)
                {
                    rb.linearVelocity = new Vector2(platformDelta.x / Time.deltaTime, rb.linearVelocity.y);
                }
                else
                {
                    obj.transform.position += platformDelta;
                }
            }
            else
            {
                obj.transform.position += platformDelta;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        pressingObjects.Add(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        pressingObjects.Remove(other);
    }

    public void OnPlatformEnter(Collider2D other)
    {
        if (other != null)
        {
            platformObjects.Add(other);
        }
    }

    public void OnPlatformExit(Collider2D other)
    {
        if (other != null)
        {
            platformObjects.Remove(other);
        }
    }
}