using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public enum PlayerStateType
{
    Run,
    Jump,
    Fall,
    Stuck,
    VineHanging,
    Dead // 死亡状态枚举
}

[System.Serializable]
public class GhostFrame
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector2 velocity;
    public PlayerStateType stateType;
    public bool isGrounded;
    public float timeStamp;
}

[System.Serializable]
public class GhostRecording
{
    public List<GhostFrame> frames = new List<GhostFrame>();
    public float startTime;
    [HideInInspector] public bool isRecording;
    [HideInInspector] public Vector3 startPosition;
    [HideInInspector] public Quaternion startRotation;
}

public class GhostSystem : MonoBehaviour
{
    [System.Serializable]
    public class GhostPlayer
    {
        public GhostRecording recording;
        [HideInInspector] public GameObject ghostObject;
        [HideInInspector] public int currentFrame;
        [HideInInspector] public bool isPlaying;
        [HideInInspector] public float startTime;
        [HideInInspector] public PlayerController ghostController;
    }

    [Header("基本设置")]
    public PlayerController playerController;
    public GameObject playerPrefab;
    public int maxLives = 9; // 最大生命值=最大分身数

    [Header("物理设置")]
    public string ghostTag = "Ghost";
    public LayerMask playerLayer;

    [Header("录制设置")]
    public float recordInterval = 0.05f;

    [Header("音频设置")]
    public AudioClip actionSound; // 通用动作音效
    [Range(0, 1)] public float soundVolume = 1f;
    private AudioSource audioSource;

    [Header("调试信息")]
    [SerializeField] private List<GhostPlayer> activeGhosts = new List<GhostPlayer>();
    [SerializeField] private GhostRecording currentRecording;
    [SerializeField] private bool isRecording;
    [SerializeField] public int currentLives; // 当前生命值=剩余可分身数
    private float lastRecordTime;

    private void Start()
    {
        currentLives = maxLives; // 初始化生命值
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Ghost"),
            LayerMask.NameToLayer("Player"),
            true
        );

        // 初始化音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    void Update()
    {
        // 核心逻辑：生命值≤0且未处于死亡状态时触发死亡
        if (currentLives <= 0 && playerController.currentState != playerController.deadState)
        {
            TriggerPlayerDeath();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame && playerController.currentState != playerController.deadState)
        {
            ToggleRecording();
        }

        if (isRecording && playerController.currentState != playerController.deadState)
        {
            TryRecordFrame();
        }

        UpdateAllGhosts();
    }

    // 触发死亡状态
    private void TriggerPlayerDeath()
    {
        Debug.Log("生命值耗尽，进入死亡状态");
        // 切换到死亡状态（音效在PlayerDeadState的OnEnter中播放）
        playerController.SwitchState(playerController.deadState);
    }

    void ToggleRecording()
    {
        if (isRecording)
        {
            StopRecording();
            if (currentRecording.frames.Count > 0)
            {
                CreateGhost();
                TeleportPlayerToStartPosition();
            }
        }
        else if (CanStartRecording())
        {
            StartRecording();
        }
    }

    void TeleportPlayerToStartPosition()
    {
        if (currentRecording != null)
        {
            playerController.transform.position = currentRecording.startPosition;
            playerController.transform.rotation = currentRecording.startRotation;
            playerController.rb.linearVelocity = Vector2.zero;
        }
    }

    bool CanStartRecording()
    {
        return currentLives > 0 && activeGhosts.Count < maxLives;
    }

    void StartRecording()
    {
        currentRecording = new GhostRecording
        {
            startTime = Time.time,
            isRecording = true,
            startPosition = playerController.transform.position,
            startRotation = playerController.transform.rotation
        };
        isRecording = true;
        lastRecordTime = Time.time;
        Debug.Log("开始录制动作");
        PlayActionSound();
    }

    void TryRecordFrame()
    {
        if (Time.time - lastRecordTime >= recordInterval)
        {
            RecordFrame();
            lastRecordTime = Time.time;
        }
    }

    void RecordFrame()
    {
        var frame = new GhostFrame
        {
            position = playerController.transform.position,
            rotation = playerController.transform.rotation,
            velocity = playerController.rb.linearVelocity,
            stateType = ConvertStateToEnum(playerController.currentState),
            isGrounded = playerController.physicsCheck.isGround,
            timeStamp = Time.time - currentRecording.startTime
        };
        currentRecording.frames.Add(frame);
    }

    // 完整的状态转换方法（包含死亡状态）
    PlayerStateType ConvertStateToEnum(PlayerState state)
    {
        if (state is PlayerRunState) return PlayerStateType.Run;
        if (state is PlayerJumpState) return PlayerStateType.Jump;
        if (state is PlayerFallState) return PlayerStateType.Fall;
        if (state is PlayerStuckState) return PlayerStateType.Stuck;
        if (state is PlayerVineHangingState) return PlayerStateType.VineHanging;
        if (state is PlayerDeadState) return PlayerStateType.Dead; // 死亡状态转换
        return PlayerStateType.Run; // 默认状态
    }

    void StopRecording()
    {
        isRecording = false;
        currentRecording.isRecording = false;
        Debug.Log($"停止录制，共录制了 {currentRecording.frames.Count} 帧动作");
    }

    // 创建分身时减少生命值
    void CreateGhost()
    {
        if (currentLives <= 0)
        {
            Debug.LogWarning("生命值不足，无法创建分身！");
            return;
        }

        currentLives--;
        Debug.Log($"创建分身，当前生命值: {currentLives}/{maxLives}");

        // 创建分身时播放音效
        PlayActionSound();

        GameObject ghostObj = Instantiate(
            playerPrefab,
            currentRecording.frames[0].position,
            currentRecording.frames[0].rotation
        );
        ghostObj.name = "Ghost_" + Time.time.ToString("F2");

        ghostObj.tag = ghostTag;
        ghostObj.layer = LayerMask.NameToLayer("Ghost");

        var rb = ghostObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        var ghostController = ghostObj.GetComponent<PlayerController>();
        if (ghostController != null)
        {
            ghostController.enabled = false;
        }

        var inputComponents = ghostObj.GetComponents<MonoBehaviour>();
        foreach (var comp in inputComponents)
        {
            if (comp is PlayerInput || comp is PlayerInputControl)
            {
                Destroy(comp);
            }
        }

        ghostObj.AddComponent<GhostCollisionHandler>().Initialize(playerLayer);

        GhostPlayer ghost = new GhostPlayer
        {
            recording = currentRecording,
            ghostObject = ghostObj,
            isPlaying = true,
            currentFrame = 0,
            startTime = Time.time,
            ghostController = ghostController
        };

        var replayer = ghostObj.AddComponent<GhostReplayer>();
        replayer.Initialize(ghost);

        activeGhosts.Add(ghost);
    }

    // 播放动作音效
    private void PlayActionSound()
    {
        if (actionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(actionSound, soundVolume);
        }
    }

    void UpdateAllGhosts()
    {
        for (int i = activeGhosts.Count - 1; i >= 0; i--)
        {
            var ghost = activeGhosts[i];
            if (!ghost.isPlaying) continue;

            UpdateGhost(ghost);

            if (ghost.currentFrame >= ghost.recording.frames.Count)
            {
                ResetGhostPlayback(ghost);
            }
        }
    }

    void UpdateGhost(GhostPlayer ghost)
    {
        float currentTime = Time.time - ghost.startTime;

        while (ghost.currentFrame < ghost.recording.frames.Count - 1 &&
               ghost.recording.frames[ghost.currentFrame + 1].timeStamp <= currentTime)
        {
            ghost.currentFrame++;
        }

        GhostFrame frame = ghost.recording.frames[ghost.currentFrame];

        ghost.ghostObject.transform.position = frame.position;
        ghost.ghostObject.transform.rotation = frame.rotation;

        if (ghost.ghostController != null && ghost.ghostController.rb != null)
        {
            ghost.ghostController.rb.linearVelocity = frame.velocity;
        }

        if (ghost.ghostController != null && ghost.ghostController.physicsCheck != null)
        {
            ghost.ghostController.physicsCheck.isGround = frame.isGrounded;
        }

        if (ghost.ghostController != null)
        {
            ForcePlayerState(ghost.ghostController, frame.stateType);
        }
    }

    // 强制设置分身状态（包含死亡状态）
    void ForcePlayerState(PlayerController controller, PlayerStateType stateType)
    {
        PlayerState newState = null;

        switch (stateType)
        {
            case PlayerStateType.Run:
                newState = new PlayerRunState();
                break;
            case PlayerStateType.Jump:
                newState = new PlayerJumpState();
                break;
            case PlayerStateType.Fall:
                newState = new PlayerFallState();
                break;
            case PlayerStateType.Stuck:
                newState = new PlayerStuckState();
                break;
            case PlayerStateType.VineHanging:
                newState = new PlayerVineHangingState();
                break;
            case PlayerStateType.Dead:  // 处理死亡状态
                newState = new PlayerDeadState();
                break;
        }

        if (newState != null && controller.currentState.GetType() != newState.GetType())
        {
            controller.SwitchState(newState);
        }
    }

    void ResetGhostPlayback(GhostPlayer ghost)
    {
        ghost.currentFrame = 0;
        ghost.startTime = Time.time;
    }

    public void RemoveAllGhosts()
    {
        foreach (var ghost in activeGhosts)
        {
            if (ghost.ghostObject != null)
            {
                Destroy(ghost.ghostObject);
            }
        }
        activeGhosts.Clear();
        currentLives = maxLives; // 重置生命值
        Debug.Log("所有分身已清除，生命值重置");
    }
}

public class GhostCollisionHandler : MonoBehaviour
{
    private LayerMask playerLayer;

    public void Initialize(LayerMask playerLayer)
    {
        this.playerLayer = playerLayer;
        StartCoroutine(DelayedCollisionSetup());
    }

    IEnumerator DelayedCollisionSetup()
    {
        yield return new WaitForFixedUpdate();

        int ghostLayer = LayerMask.NameToLayer("Ghost");
        int playerLayerID = (int)Mathf.Log(playerLayer.value, 2);
        Physics2D.IgnoreLayerCollision(ghostLayer, playerLayerID, true);

        foreach (var ghost in GameObject.FindGameObjectsWithTag("Ghost"))
        {
            if (ghost != gameObject)
            {
                var myColliders = GetComponents<Collider2D>();
                var otherColliders = ghost.GetComponents<Collider2D>();

                foreach (var myCol in myColliders)
                {
                    foreach (var otherCol in otherColliders)
                    {
                        Physics2D.IgnoreCollision(myCol, otherCol, true);
                    }
                }
            }
        }
    }
}

public class GhostReplayer : MonoBehaviour
{
    private GhostSystem.GhostPlayer ghostData;

    public void Initialize(GhostSystem.GhostPlayer data)
    {
        ghostData = data;
    }

    void Update()
    {
        // 分身重放逻辑由GhostSystem统一管理
    }
}
