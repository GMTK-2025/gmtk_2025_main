using UnityEngine;

public class PlayerClimbState : PlayerState
{
    private Transform climbableObject;
    private bool isClimbing = true;
    private Vector3 climbOffset;
    // 与攀爬物的基础距离
    private float minDistanceToClimbable = 2.4f;
    private float noInputTimer = 0f;
    private const float NO_INPUT_TIMEOUT = 0.1f;
    private bool isFirstEnter = true;
    // 水平移动范围倍率，控制最大可移动距离
    private float horizontalRangeMultiplier = 2.4f;

    // 攀爬物边界限制
    private float climbableHalfWidth;  // 攀爬物宽度的一半
    private float leftBound;           // 左边界
    private float rightBound;          // 右边界

    // 存储原始碰撞状态，用于退出时恢复
    private bool[] originalIgnoreStates = new bool[32];

    // 允许外部设置攀爬物体
    public void SetClimbableObject(Transform climbable)
    {
        climbableObject = climbable;
        if (climbableObject != null)
        {
            CalculateClimbableBounds();  // 计算边界
        }
    }

    public override void OnEnter(PlayerController player)
    {
        base.OnEnter(player);
        this.player = player;
        isFirstEnter = true;
        isClimbing = true;

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

        // 计算攀爬物边界
        CalculateClimbableBounds();

        // 计算偏移量
        climbOffset = player.transform.position - climbableObject.position;
        climbOffset.x = 0;
        climbOffset.z = 0;

        // 设置初始位置（确保在边界内）
        Vector3 initialPosition = climbableObject.position + climbOffset;
        initialPosition.x = Mathf.Clamp(initialPosition.x, leftBound, rightBound);
        player.transform.position = initialPosition;

        // 保存原始碰撞状态并配置攀爬时的碰撞层
        SaveOriginalCollisionStates();
        ConfigureCollisionLayers(true);
    }

    // 计算攀爬物的左右边界
    private void CalculateClimbableBounds()
    {
        if (climbableObject != null)
        {
            // 获取攀爬物的碰撞体大小
            Collider2D climbableCollider = climbableObject.GetComponent<Collider2D>();
            if (climbableCollider != null)
            {
                // 计算攀爬物宽度的一半
                Bounds bounds = climbableCollider.bounds;
                climbableHalfWidth = bounds.extents.x * horizontalRangeMultiplier;

                // 计算左右边界
                leftBound = climbableObject.position.x - climbableHalfWidth;
                rightBound = climbableObject.position.x + climbableHalfWidth;
            }
            else
            {
                // 如果没有碰撞体，使用默认值
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

        // 跳跃处理
        if (player.inputControl.Player.Jump.triggered)
        {
            ExitClimbingWithJump();
            return;
        }

        if (noInputTimer < NO_INPUT_TIMEOUT)
        {
            return;
        }

        // 验证攀爬物体
        if (climbableObject == null || !CheckStillClimbing())
        {
            ExitClimbing();
            return;
        }

        ProcessClimbingInput();
    }

    private void ProcessClimbingInput()
    {
        Vector2 input = player.inputDirection;

        // 垂直移动
        if (Mathf.Abs(input.y) > 0.05f)
        {
            player.rb.linearVelocity = new Vector2(0, input.y * player.climbSpeed);
        }
        else
        {
            player.rb.linearVelocity = new Vector2(0, 0);
        }

        // 水平移动 - 使用统一边界限制
        if (Mathf.Abs(input.x) > 0.05f)
        {
            // 计算水平移动量
            float moveAmount = input.x * player.speed * 0.9f * Time.deltaTime;
            Vector3 newPosition = player.transform.position;
            newPosition.x += moveAmount;

            // 使用预计算的左右边界限制水平移动
            newPosition.x = Mathf.Clamp(newPosition.x, leftBound, rightBound);
            player.transform.position = newPosition;
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
            CalculateClimbableBounds();  // 找到攀爬物后计算边界
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
    }

    public override void OnExit(PlayerController player)
    {
        isClimbing = false;
        player.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        player.rb.gravityScale = player.normalGravityScale;
        RestoreOriginalCollisionStates();
        noInputTimer = 0f;
    }

    public override void PhysicsUpdate()
    {
        // 攀爬状态下物理更新由刚体自身处理
    }

    private void SaveOriginalCollisionStates()
    {
        if (player == null) return;

        int playerLayer = player.gameObject.layer;
        for (int i = 0; i < 32; i++)
        {
            originalIgnoreStates[i] = Physics2D.GetIgnoreLayerCollision(playerLayer, i);
        }
    }

    private void RestoreOriginalCollisionStates()
    {
        if (player == null) return;

        int playerLayer = player.gameObject.layer;
        for (int i = 0; i < 32; i++)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, i, originalIgnoreStates[i]);
        }
    }

    private void ConfigureCollisionLayers(bool isClimbing)
    {
        if (player == null) return;

        int playerLayer = player.gameObject.layer;

        // 获取需要忽略的层
        int obstacleLayer = LayerMask.NameToLayer(player.obstacleLayerName);
        int groundLayer = LayerMask.NameToLayer(player.groundLayerName);
        int playerLayerMask = LayerMask.NameToLayer(player.playerLayerName);

        // 设置攀爬时的碰撞规则
        for (int i = 0; i < 32; i++)
        {
            if (i == obstacleLayer || i == groundLayer || i == playerLayerMask)
            {
                Physics2D.IgnoreLayerCollision(playerLayer, i, isClimbing);
            }
        }
    }
}
