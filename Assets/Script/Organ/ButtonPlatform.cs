using System.Collections.Generic;
using UnityEngine;

public class ButtonPlatform : MonoBehaviour
{
    [Header("平台基础设置")]
    public Transform platform;              // 平台物体
    public float platformMoveDistance = 5f; // 左移距离
    public float buttonPressDepth = 0.2f;   // 按钮按下深度
    public float moveSpeed = 5f;            // 移动速度

    [Header("检测优化设置")]
    [Tooltip("边缘检测缓冲范围（加大至0.3~0.5）")]
    public float edgeDetectionBuffer = 0.3f;
    [Tooltip("离开判定延迟（加大至0.5秒）")]
    public float stateChangeDelay = 0.5f;

    [Header("音频设置")]
    public AudioClip buttonPressSound;
    [Range(0, 1)] public float soundVolume = 1f; 
    private AudioSource audioSource; 

    private Vector3 originalPlatformPosition;
    private Vector3 leftPosition;
    private Vector3 originalButtonPosition;

    private Dictionary<Collider2D, float> platformObjects = new Dictionary<Collider2D, float>();
    private HashSet<Collider2D> pressingObjects = new HashSet<Collider2D>();
    private Collider2D platformCollider;

    // 用于判断是否刚触发按钮（避免重复播放音效）
    private bool wasPressedLastFrame = false;

    void Start()
    {
        if (platform == null)
        {
            Debug.LogError("平台未赋值！", this);
            return;
        }
        originalPlatformPosition = platform.position;
        leftPosition = originalPlatformPosition + Vector3.left * platformMoveDistance;
        originalButtonPosition = transform.position;
        platformCollider = platform.GetComponent<Collider2D>();

        // 强制开启平台碰撞器的触发器
        if (platformCollider != null && !platformCollider.isTrigger)
        {
            platformCollider.isTrigger = true;
        }

        // 初始化音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0;
    }

    void Update()
    {
        if (platform == null) return;

        UpdatePlatformObjectStatus();

        Vector3 targetPos;
        bool hasObjects = platformObjects.Count > 0;
        bool isButtonPressed = pressingObjects.Count > 0;

        // 检测按钮状态变化（从未按下→按下）时播放音效
        if (isButtonPressed && !wasPressedLastFrame)
        {
            PlayButtonPressSound();
        }
        // 更新上一帧状态
        wasPressedLastFrame = isButtonPressed;

        if (hasObjects)
        {
            targetPos = originalPlatformPosition;
        }
        else
        {
            targetPos = isButtonPressed ? leftPosition : originalPlatformPosition;
        }

        // 移动平台（限制单次移动距离，避免抖动）
        float maxStep = moveSpeed * Time.deltaTime;
        platform.position = Vector3.MoveTowards(platform.position, targetPos, maxStep);

        // 移动按钮
        Vector3 targetButtonPos = isButtonPressed
            ? originalButtonPosition + Vector3.down * buttonPressDepth
            : originalButtonPosition;
        transform.position = Vector3.MoveTowards(transform.position, targetButtonPos, maxStep);
    }

    // 新增：播放按钮触发音效
    private void PlayButtonPressSound()
    {
        if (buttonPressSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonPressSound, soundVolume);
            Debug.Log("按钮被触发，播放音效");
        }
        else if (buttonPressSound == null)
        {
            Debug.LogWarning("未设置按钮音效，请在Inspector中赋值buttonPressSound", this);
        }
    }

    private void UpdatePlatformObjectStatus()
    {
        List<Collider2D> toRemove = new List<Collider2D>();
        var currentKeys = new List<Collider2D>(platformObjects.Keys);

        foreach (var obj in currentKeys)
        {
            if (!platformObjects.ContainsKey(obj) || obj == null)
            {
                toRemove.Add(obj);
                continue;
            }

            if (IsObjectOnPlatform(obj))
            {
                platformObjects[obj] = 0;
            }
            else
            {
                platformObjects[obj] += Time.deltaTime;
                if (platformObjects[obj] >= stateChangeDelay)
                {
                    toRemove.Add(obj);
                }
            }
        }

        foreach (var obj in toRemove)
        {
            if (platformObjects.ContainsKey(obj))
                platformObjects.Remove(obj);
        }
    }

    // 扩大检测范围+检查底部位置
    private bool IsObjectOnPlatform(Collider2D objCollider)
    {
        if (platformCollider == null || objCollider == null)
            return false;

        Bounds platformBounds = platformCollider.bounds;
        platformBounds.Expand(edgeDetectionBuffer);

        // 检测物体底部中心是否在平台范围内
        Bounds objBounds = objCollider.bounds;
        Vector3 objBottomCenter = new Vector3(objBounds.center.x, objBounds.min.y, objBounds.center.z);

        return platformBounds.Contains(objBottomCenter);
    }

    // 按钮触发（进入触发器）
    private void OnTriggerEnter2D(Collider2D other) => pressingObjects.Add(other);

    // 按钮离开（退出触发器）
    private void OnTriggerExit2D(Collider2D other) => pressingObjects.Remove(other);

    // 平台触发（进入）
    public void OnPlatformEnter(Collider2D other)
    {
        if (!platformObjects.ContainsKey(other))
        {
            platformObjects.Add(other, 0);
        }
        else
        {
            platformObjects[other] = 0;
        }
    }

    // 平台触发（退出）
    public void OnPlatformExit(Collider2D other)
    {
        // 退出时不立即移除，交给UpdatePlatformObjectStatus延迟处理
    }
}