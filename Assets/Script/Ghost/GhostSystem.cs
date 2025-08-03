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
    Dead
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
}

public class GhostSystem : MonoBehaviour
{
    [System.Serializable]
    public class GhostPlayer
    {
        public GhostRecording recording;
        public GameObject ghostObject;
        public int currentFrame;
        public bool isPlaying;
        public float startTime;
        public PlayerController ghostController;
    }

    [Header("基本设置")]
    public PlayerController playerController;
    public GameObject playerPrefab;
    public int maxLives = 9;
    public string ghostTag = "Ghost";
    public LayerMask playerLayer;

    [Header("录制设置")]
    public float recordInterval = 0.05f;

    [Header("音频设置")]
    public AudioClip actionSound;
    [Range(0, 1)] public float soundVolume = 1f;
    private AudioSource audioSource;

    [Header("调试信息")]
    [SerializeField] private List<GhostPlayer> activeGhosts = new List<GhostPlayer>();
    [SerializeField] private GhostRecording currentRecording;
    [SerializeField] private bool isRecording;
    [SerializeField] public int currentLives;
    private float lastRecordTime;
    private bool recordingQueued;
    //currentlives
    private void Start()
    {
        currentLives = maxLives;
        isRecording = false;
        recordingQueued = false;

        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Ghost"),
            LayerMask.NameToLayer("Player"),
            true
        );

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void Update()
    {
        if (currentLives <= 0 && playerController.currentState != playerController.deadState)
        {
            TriggerPlayerDeath();
            return;
        }

        HandleRecordingInput();

        if (isRecording)
        {
            TryRecordFrame();
        }

        UpdateAllGhosts();
    }

    private bool CanStartRecording()
    {
        return currentLives > 0 && activeGhosts.Count < maxLives;
    }

    private void HandleRecordingInput()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame && playerController.currentState != playerController.deadState)
        {
            if (!isRecording && !recordingQueued && CanStartRecording())
            {
                recordingQueued = true;
                StartCoroutine(StartRecordingDelayed());
            }
            else if (isRecording)
            {
                StopRecording();
                if (currentRecording.frames.Count > 0)
                {
                    CreateGhost();
                }
            }
        }
    }

    private IEnumerator StartRecordingDelayed()
    {
        yield return null;
        StartRecording();
        recordingQueued = false;
    }

    private void StartRecording()
    {
        currentRecording = new GhostRecording
        {
            startTime = Time.time,
            isRecording = true
        };
        isRecording = true;
        lastRecordTime = Time.time;
        Debug.Log("开始录制动作");
        PlayActionSound();
    }

    private void StopRecording()
    {
        isRecording = false;
        currentRecording.isRecording = false;
        Debug.Log($"停止录制，共录制了 {currentRecording.frames.Count} 帧动作");
    }

    private void TryRecordFrame()
    {
        if (Time.time - lastRecordTime >= recordInterval)
        {
            RecordFrame();
            lastRecordTime = Time.time;
        }
    }

    private void RecordFrame()
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

    private PlayerStateType ConvertStateToEnum(PlayerState state)
    {
        if (state is PlayerRunState) return PlayerStateType.Run;
        if (state is PlayerJumpState) return PlayerStateType.Jump;
        if (state is PlayerFallState) return PlayerStateType.Fall;
        if (state is PlayerStuckState) return PlayerStateType.Stuck;
        if (state is PlayerVineHangingState) return PlayerStateType.VineHanging;
        if (state is PlayerDeadState) return PlayerStateType.Dead;
        return PlayerStateType.Run;
    }

    private void CreateGhost()
    {
        if (currentLives <= 0) return;

        currentLives--;
        Debug.Log($"创建分身，当前生命值: {currentLives}/{maxLives}");

        PlayActionSound();

        GameObject ghostObj = Instantiate(
            playerPrefab,
            currentRecording.frames[0].position,
            currentRecording.frames[0].rotation
        );
        ghostObj.name = $"Ghost_{Time.time:F2}";

        SetupGhostObject(ghostObj);

        GhostPlayer ghost = new GhostPlayer
        {
            recording = currentRecording,
            ghostObject = ghostObj,
            isPlaying = true,
            currentFrame = 0,
            startTime = Time.time,
            ghostController = ghostObj.GetComponent<PlayerController>()
        };

        ghost.ghostController.enabled = false;
        ghostObj.AddComponent<GhostCollisionHandler>().Initialize(playerLayer);
        ghostObj.AddComponent<GhostReplayer>().Initialize(ghost);

        activeGhosts.Add(ghost);
        CheckGhostLimit();
    }

    private void SetupGhostObject(GameObject ghostObj)
    {
        ghostObj.tag = ghostTag;
        ghostObj.layer = LayerMask.NameToLayer("Ghost");

        var rb = ghostObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        var inputComponents = ghostObj.GetComponents<MonoBehaviour>();
        foreach (var comp in inputComponents)
        {
            if (comp is PlayerInput || comp is PlayerInputControl)
            {
                Destroy(comp);
            }
        }
    }

    private void UpdateAllGhosts()
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

    private void UpdateGhost(GhostPlayer ghost)
    {
        float currentTime = Time.time - ghost.startTime;
        var frames = ghost.recording.frames;

        while (ghost.currentFrame < frames.Count - 1 &&
               frames[ghost.currentFrame + 1].timeStamp <= currentTime)
        {
            ghost.currentFrame++;
        }

        GhostFrame frame = frames[ghost.currentFrame];
        ghost.ghostObject.transform.SetPositionAndRotation(frame.position, frame.rotation);

        if (ghost.ghostController != null)
        {
            if (ghost.ghostController.rb != null)
            {
                ghost.ghostController.rb.linearVelocity = frame.velocity;
            }

            if (ghost.ghostController.physicsCheck != null)
            {
                ghost.ghostController.physicsCheck.isGround = frame.isGrounded;
            }

            ForcePlayerState(ghost.ghostController, frame.stateType);
        }
    }

    private void ForcePlayerState(PlayerController controller, PlayerStateType stateType)
    {
        PlayerState newState = stateType switch
        {
            PlayerStateType.Run => new PlayerRunState(),
            PlayerStateType.Jump => new PlayerJumpState(),
            PlayerStateType.Fall => new PlayerFallState(),
            PlayerStateType.Stuck => new PlayerStuckState(),
            PlayerStateType.VineHanging => new PlayerVineHangingState(),
            PlayerStateType.Dead => new PlayerDeadState(),
            _ => new PlayerRunState()
        };

        if (controller.currentState.GetType() != newState.GetType())
        {
            controller.SwitchState(newState);
        }
    }

    private void ResetGhostPlayback(GhostPlayer ghost)
    {
        ghost.currentFrame = 0;
        ghost.startTime = Time.time;
    }

    private void CheckGhostLimit()
    {
        while (activeGhosts.Count > 3 && activeGhosts.Count > 0)
        {
            GhostPlayer oldestGhost = activeGhosts[0];
            foreach (var ghost in activeGhosts)
            {
                if (ghost.startTime < oldestGhost.startTime)
                {
                    oldestGhost = ghost;
                }
            }
            RecycleGhost(oldestGhost);
        }
    }

    public void RecycleGhost(GhostPlayer ghost)
    {
        if (ghost == null || ghost.ghostObject == null) return;

        Destroy(ghost.ghostObject);
        activeGhosts.Remove(ghost);
        // 移除了恢复生命值的代码
        Debug.Log($"回收分身");
    }

    private void PlayActionSound()
    {
        if (actionSound != null)
        {
            audioSource.PlayOneShot(actionSound, soundVolume);
        }
    }

    private void TriggerPlayerDeath()
    {
        Debug.Log("生命值耗尽，进入死亡状态");
        playerController.SwitchState(playerController.deadState);
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
        currentLives = maxLives;
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
                foreach (var myCol in GetComponents<Collider2D>())
                {
                    foreach (var otherCol in ghost.GetComponents<Collider2D>())
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
}
