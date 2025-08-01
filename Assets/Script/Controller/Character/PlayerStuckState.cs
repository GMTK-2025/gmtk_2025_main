using UnityEngine;

public class PlayerStuckState : PlayerState
{
    private float stuckTimer;
    private Transform anchorPoint;
    private bool isUsingTempPoint;
    private float playerHeight;
    private Vector3 positionOffset;


    public void SetTargetPoint(Transform point)
    {
        if (player == null || point == null) return;

        CleanupTempPoint();

        anchorPoint = point;
        isUsingTempPoint = false;
        CalculatePlayerHeight();
        CalculateOffset();
    }

    public void CreateAndSetTargetPoint(Transform parent, Vector2 localPos)
    {
        if (player == null || parent == null) return;

        CleanupTempPoint();

        GameObject tempPoint = new GameObject("TempAnchorPoint");
        tempPoint.transform.SetParent(parent);
        tempPoint.transform.localPosition = localPos;

        anchorPoint = tempPoint.transform;
        isUsingTempPoint = true;
        CalculatePlayerHeight();
        CalculateOffset();
    }

    private void CalculatePlayerHeight()
    {
        if (player == null) return;

        var collider = player.GetComponent<Collider2D>();
        playerHeight = collider != null ? collider.bounds.size.y : 1f;
    }

    private void CalculateOffset()
    {
        positionOffset = new Vector3(0, -playerHeight, 0);
    }

    private void CleanupTempPoint()
    {
        if (isUsingTempPoint && anchorPoint != null)
            Object.Destroy(anchorPoint.gameObject);
    }

    public override void OnEnter(PlayerController player)
    {
        this.player = player;
        stuckTimer = player.stuckDuration;

        player.rb.bodyType = RigidbodyType2D.Kinematic;
        player.rb.gravityScale = 0;
        player.rb.linearVelocity = Vector2.zero;
    }

    public override void LogicUpdate()
    {
        if (player == null) return;

        stuckTimer -= Time.deltaTime;
        if (stuckTimer <= 0 || player.inputControl.Player.Jump.WasPressedThisFrame())
        {
            EscapeStuck();
        }
    }

    public override void PhysicsUpdate()
    {
        if (player != null && anchorPoint != null)
        {
            player.transform.position = anchorPoint.position + positionOffset;
            player.rb.linearVelocity = Vector2.zero;
        }
    }

    private void EscapeStuck()
    {
        if (player == null) return;


        player.rb.bodyType = RigidbodyType2D.Dynamic;
        player.rb.gravityScale = player.normalGravityScale;

        Vector2 escapeVelocity = Vector2.up * player.jumpForce * 0.3f;
        if (player.inputDirection.x != 0)
            escapeVelocity.x = player.inputDirection.x * player.speed * 0.5f;

        player.rb.linearVelocity = escapeVelocity;
        player.SwitchState(player.physicsCheck.isGround ? player.runState : player.fallState);
    }

    public override void OnExit(PlayerController player)
    {
        CleanupTempPoint();
        anchorPoint = null;
    }
}