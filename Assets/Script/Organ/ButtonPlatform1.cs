using System.Collections.Generic;
using UnityEngine;

public class ButtonPlatform1 : MonoBehaviour
{
    [Header("平台设置")]
    public Transform platform; // 平台物体
    public float platformMoveDistance = 3f; // 平台下移距离
    public float buttonPressDepth = 0.2f; // 按钮按下深度
    [Range(1f, 3f)] public float moveSpeed = 2f; // 平台移动速度
    public float antiBounceThreshold = 0.05f; // 位置偏差阈值
    public float horizontalFriction = 0.1f; // 水平移动摩擦力

    private Vector3 originalPlatformPosition;
    private Vector3 targetDownPosition;
    private Vector3 originalButtonPosition;
    private HashSet<Collider2D> pressingObjects = new HashSet<Collider2D>();
    private HashSet<Collider2D> platformObjects = new HashSet<Collider2D>();
    private bool isButtonPressed; // 按钮是否被按下（物体未移开则保持）
    private Rigidbody2D platformRb;
    private Dictionary<Collider2D, Vector3> objectOffsets = new Dictionary<Collider2D, Vector3>();
    private bool isProcessingCollision;

    // 物理材质
    private PhysicsMaterial2D platformMaterial;

    void Start()
    {
        originalPlatformPosition = platform.position;
        targetDownPosition = originalPlatformPosition + Vector3.down * platformMoveDistance;
        originalButtonPosition = transform.position;

        platformMaterial = new PhysicsMaterial2D("PlatformMaterial");
        platformMaterial.friction = horizontalFriction;
        platformMaterial.bounciness = 0f;

        platformRb = platform.GetComponent<Rigidbody2D>();
        if (platformRb != null)
        {
            platformRb.bodyType = RigidbodyType2D.Kinematic;
            platformRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        ApplyPlatformMaterial(platform.gameObject);
        ForceCheckColliders();
    }

    void FixedUpdate()
    {
        isProcessingCollision = true;

        // 按钮逻辑：物体未移开则保持按下
        bool hasPressingObjects = pressingObjects.Count > 0;
        isButtonPressed = hasPressingObjects;

        // 平台逻辑：按钮按下时下移，平台有物体时强制上升
        bool hasPlatformObjects = platformObjects.Count > 0;
        Vector3 targetPlatformPos = isButtonPressed ? targetDownPosition : originalPlatformPosition;

        if (hasPlatformObjects)
        {
            targetPlatformPos = originalPlatformPosition; // 有物体时强制回原位
        }

        // 平台移动
        Vector3 direction = (targetPlatformPos - platform.position).normalized;
        float distanceToTarget = Vector3.Distance(platform.position, targetPlatformPos);

        if (distanceToTarget > moveSpeed * Time.fixedDeltaTime)
        {
            platform.Translate(direction * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            platform.position = targetPlatformPos;
        }

        // 按钮移动（根据 isButtonPressed 状态，物体在则保持下沉）
        Vector3 targetButtonPos = isButtonPressed ?
            originalButtonPosition + Vector3.down * buttonPressDepth :
            originalButtonPosition;
        transform.position = Vector3.Lerp(transform.position, targetButtonPos, moveSpeed * 0.5f * Time.fixedDeltaTime);

        // 物体跟随平台（仅 Y 轴）
        if (hasPlatformObjects)
        {
            SyncObjectsWithPlatform();
        }

        isProcessingCollision = false;
    }

    private void SyncObjectsWithPlatform()
    {
        foreach (var col in platformObjects)
        {
            if (col == null) continue;

            if (!objectOffsets.ContainsKey(col))
            {
                Vector3 offset = col.transform.position - platform.position;
                objectOffsets[col] = new Vector3(0, offset.y, 0);
                ApplyPlatformMaterial(col.gameObject);
            }

            Vector3 targetPosition = new Vector3(
                col.transform.position.x,
                platform.position.y + objectOffsets[col].y,
                col.transform.position.z
            );

            Vector3 positionDelta = targetPosition - col.transform.position;

            if (Mathf.Abs(positionDelta.y) > antiBounceThreshold)
            {
                Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
                if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
                {
                    Vector2 newVelocity = new Vector2(
                        rb.linearVelocity.x,
                        positionDelta.y / Time.fixedDeltaTime
                    );
                    rb.linearVelocity = newVelocity;
                }
                else
                {
                    col.transform.position = new Vector3(
                        col.transform.position.x,
                        Mathf.Lerp(col.transform.position.y, targetPosition.y, 0.8f),
                        col.transform.position.z
                    );
                }
            }
        }
    }

    private void ApplyPlatformMaterial(GameObject obj)
    {
        Collider2D[] colliders = obj.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            if (!col.isTrigger)
            {
                col.sharedMaterial = platformMaterial;
            }
        }
    }

    public void OnPlatformEnter(Collider2D other)
    {
        if (other != null && !platformObjects.Contains(other))
        {
            platformObjects.Add(other);
            Vector3 offset = other.transform.position - platform.position;
            objectOffsets[other] = new Vector3(0, offset.y, 0);
            ApplyPlatformMaterial(other.gameObject);
        }
    }

    public void OnPlatformExit(Collider2D other)
    {
        if (other != null && platformObjects.Contains(other))
        {
            platformObjects.Remove(other);
            objectOffsets.Remove(other);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isProcessingCollision) return;
        pressingObjects.Add(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (isProcessingCollision) return;
        pressingObjects.Remove(other);
    }

    private void ForceCheckColliders()
    {
        PlatformDetector detector = platform.GetComponent<PlatformDetector>();
        if (detector != null)
        {
            Collider2D detectCollider = detector.GetComponent<Collider2D>();
            if (detectCollider != null && !detectCollider.isTrigger)
            {
                detectCollider.isTrigger = true;
            }
        }

        Collider2D[] allColliders = platform.GetComponents<Collider2D>();
        bool hasPhysicalCollider = false;
        foreach (var col in allColliders)
        {
            if (!col.isTrigger)
            {
                hasPhysicalCollider = true;
                break;
            }
        }
    }
}
