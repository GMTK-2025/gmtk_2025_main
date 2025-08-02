using UnityEngine;

public class PlayerClimbState : PlayerState
{
    private Transform climbableObject;
    private bool isClimbing = true;
    private Vector3 climbOffset;
    private float minDistanceToClimbable = 2.4f;
    private float noInputTimer = 0f;
    private const float NO_INPUT_TIMEOUT = 0.1f;
    private bool isFirstEnter = true;
    private float horizontalRangeMultiplier = 2.4f;

    // 攀爬物边界限制
    private float climbableHalfWidth;
    private float leftBound;
    private float rightBound;

    // 存储原始碰撞状态
    private bool[] originalIgnoreStates = new bool[32];

    // 攀爬音效控制（核心变量）
    private bool isPlayingClimbSound = false; // 当前是否在播放循环音效
    private bool isMoving = false; // 当前帧是否在移动

    // 允许外部设置攀爬物体
    public void SetClimbableObject(Transform climbable)
    {
        climbableObject = climbable;
        if (climbableObject != null)
        {
            CalculateClimbableBounds();
        }
    }

    public override void OnEnter(PlayerController player)
    {
        base.OnEnter(player);
        this.player = player;
        isFirstEnter = true;
        isClimbing = true;

        // 初始化音效状态
        isPlayingClimbSound = false;
        isMoving = false;

        // 物理状态重置
        player.rb.linearVelocity = Vector2.zero;
        player.rb.angularVelocity = 0f;
        player.rb.gravityScale = 0;
        player.rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;

        // 输入重置
        noInputTimer = 0f;
        player.inputDirection = Vector2.zero;

        if (climbableObject == null)
        {
            climbableObject = FindClosestClimbable();
        }

        if (climbableObject == null)
        {
            player.SwitchState(player.runState);
            return;
        }

        CalculateClimbableBounds();
        climbOffset = player.transform.position - climbableObject.position;
        climbOffset.x = 0;
        climbOffset.z = 0;

        Vector3 initialPosition = climbableObject.position + climbOffset;
        initialPosition.x = Mathf.Clamp(initialPosition.x, leftBound, rightBound);
        player.transform.position = initialPosition;

        SaveOriginalCollisionStates();
        //ConfigureCollisionLayers(true);
    }

    private void CalculateClimbableBounds()
    {
        if (climbableObject != null)
        {
            Collider2D climbableCollider = climbableObject.GetComponent<Collider2D>();
            if (climbableCollider != null)
            {
                Bounds bounds = climbableCollider.bounds;
                climbableHalfWidth = bounds.extents.x * horizontalRangeMultiplier;
                leftBound = climbableObject.position.x - climbableHalfWidth;
                rightBound = climbableObject.position.x + climbableHalfWidth;
            }
            else
            {
                climbableHalfWidth = 1.0f * horizontalRangeMultiplier;
                leftBound = climbableObject.position.x - climbableHalfWidth;
                rightBound = climbableObject.position.x + climbableHalfWidth;
            }
        }
    }

    public override void LogicUpdate()
    {
        if (!isClimbing) return;

        if (isFirstEnter)
        {
            noInputTimer = NO_INPUT_TIMEOUT;
            isFirstEnter = false;
        }
        else
        {
            noInputTimer += Time.deltaTime;
        }

        // 跳跃退出攀爬
        if (player.inputControl.Player.Jump.triggered)
        {
            ExitClimbingWithJump();
            return;
        }

        if (noInputTimer < NO_INPUT_TIMEOUT)
        {
            return;
        }

        // 检查是否仍在攀爬范围内
        if (climbableObject == null || !CheckStillClimbing())
        {
            ExitClimbing();
            return;
        }

        ProcessClimbingInput();
        UpdateClimbSound(); // 处理音效逻辑
    }

    private void ProcessClimbingInput()
    {
        Vector2 input = player.inputDirection;
        isMoving = false; // 重置移动状态

        // 垂直移动检测（按住上/下方向键）
        if (Mathf.Abs(input.y) > 0.05f)
        {
            player.rb.linearVelocity = new Vector2(0, input.y * player.climbSpeed);
            isMoving = true; // 标记为移动状态
        }
        else
        {
            player.rb.linearVelocity = new Vector2(0, 0);
        }

        // 水平移动检测（按住左/右方向键）
        if (Mathf.Abs(input.x) > 0.05f)
        {
            float moveAmount = input.x * player.speed * 0.9f * Time.deltaTime;
            Vector3 newPosition = player.transform.position;
            newPosition.x += moveAmount;
            newPosition.x = Mathf.Clamp(newPosition.x, leftBound, rightBound);
            player.transform.position = newPosition;
            isMoving = true; // 标记为移动状态
        }
    }

    // 音效控制核心逻辑
    private void UpdateClimbSound()
    {
        // 移动时播放循环音效
        if (isMoving && !isPlayingClimbSound)
        {
            player.PlayLoopSound(player.climbSound);
            isPlayingClimbSound = true;
        }
        // 停止移动时停止音效
        else if (!isMoving && isPlayingClimbSound)
        {
            player.StopLoopSound();
            isPlayingClimbSound = false;
        }
    }

    private Transform FindClosestClimbable()
    {
        if (player == null) return null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            player.transform.position,
            player.climbCheckDistance * 1.5f,
            player.climbLayer);

        if (hits.Length == 0)
        {
            return null;
        }

        Transform closest = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.isTrigger) continue;

            float dist = Vector2.Distance(
                player.transform.position,
                hit.transform.position);

            if (dist < minDistance)
            {
                minDistance = dist;
                closest = hit.transform;
            }
        }

        if (closest != null)
        {
            CalculateClimbableBounds();
        }

        return closest;
    }

    private bool CheckStillClimbing()
    {
        if (climbableObject == null || player == null) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            player.transform.position,
            (climbableObject.position - player.transform.position).normalized,
            minDistanceToClimbable * horizontalRangeMultiplier,
            player.climbLayer);

        return hit.collider != null && hit.transform == climbableObject;
    }

    private void ExitClimbing()
    {
        if (!isClimbing) return;

        isClimbing = false;
        player.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        player.rb.gravityScale = player.normalGravityScale;
        RestoreOriginalCollisionStates();
        player.SwitchState(player.fallState);
        climbableObject = null;

        // 退出时停止音效
        if (isPlayingClimbSound)
        {
            player.StopLoopSound();
            isPlayingClimbSound = false;
        }
    }

    private void ExitClimbingWithJump()
    {
        isClimbing = false;
        player.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        player.rb.gravityScale = player.normalGravityScale;
        RestoreOriginalCollisionStates();
        player.rb.AddForce(new Vector2(0, player.jumpForce * 0.7f), ForceMode2D.Impulse);
        player.SwitchState(player.jumpState);
        climbableObject = null;

        // 退出时停止音效
        if (isPlayingClimbSound)
        {
            player.StopLoopSound();
            isPlayingClimbSound = false;
        }
    }

    public override void OnExit(PlayerController player)
    {
        isClimbing = false;
        player.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        player.rb.gravityScale = player.normalGravityScale;
        RestoreOriginalCollisionStates();
        noInputTimer = 0f;

        // 退出状态时强制停止音效
        if (isPlayingClimbSound)
        {
            player.StopLoopSound();
            isPlayingClimbSound = false;
        }
    }

    public override void PhysicsUpdate() { }

    private void SaveOriginalCollisionStates()
    {
        if (player == null) return;

        int playerLayer = player.gameObject.layer;
        for (int i = 0; i < 32; i++)
        {
            originalIgnoreStates[i] = Physics2D.GetIgnoreLayerCollision(playerLayer, i);
        }
    }

    //ConfigureCollisionLayers

    private void RestoreOriginalCollisionStates()
    {
        if (player == null) return;

        int playerLayer = player.gameObject.layer;
        for (int i = 0; i < 32; i++)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, i, originalIgnoreStates[i]);
        }
    }

    //private void ConfigureCollisionLayers(bool isClimbing)
    //{
    //    if (player == null) return;

    //    int playerLayer = player.gameObject.layer;
    //    int obstacleLayer = LayerMask.NameToLayer(player.obstacleLayerName);
    //    int groundLayer = LayerMask.NameToLayer(player.groundLayerName);
    //    int playerLayerMask = LayerMask.NameToLayer(player.playerLayerName);

    //    for (int i = 0; i < 32; i++)
    //    {
    //        if (i == obstacleLayer || i == groundLayer || i == playerLayerMask)
    //        {
    //            Physics2D.IgnoreLayerCollision(playerLayer, i, isClimbing);
    //        }
    //    }
    //}
}