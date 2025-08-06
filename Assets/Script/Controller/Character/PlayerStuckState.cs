using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStuckState : PlayerState
{
    private float stuckTimer;
    private Transform anchorPoint;
    private bool isUsingTempPoint;
    private float playerHeight;
    private Vector3 positionOffset;
    private bool hasHandledJumpInput;

    // ����ê�㣨�����������й̶��㣩
    public void SetTargetPoint(Transform point)
    {
        if (_player == null || point == null) return;
        CleanupTempPoint();
        anchorPoint = point;
        isUsingTempPoint = false;
        CalculatePlayerHeight();
        CalculateOffset();
    }

    // ��̬����ê�㣨������ʱ���ص㣩
    public void CreateAndSetTargetPoint(Transform parent, Vector2 localPos)
    {
        if (_player == null || parent == null) return;
        CleanupTempPoint();

        GameObject tempPoint = new GameObject("TempAnchorPoint");
        tempPoint.transform.SetParent(parent);
        tempPoint.transform.localPosition = localPos;

        anchorPoint = tempPoint.transform;
        isUsingTempPoint = true;
        CalculatePlayerHeight();
        CalculateOffset();
    }

    // �����ɫ�߶ȣ�����ê��ƫ�ƣ�
    private void CalculatePlayerHeight()
    {
        if (_player == null) return;
        Collider2D collider = _player.GetComponent<Collider2D>();
        playerHeight = collider != null ? collider.bounds.size.y : 1f;
    }

    // ����ê��ƫ��
    private void CalculateOffset()
    {
        positionOffset = new Vector3(0, -playerHeight, 0);
    }


    private void CleanupTempPoint()
    {
        if (isUsingTempPoint && anchorPoint != null)
        {
            //Destroy(anchorPoint.gameObject);
        }
    }

    // �����ס״̬ʱ��ʼ��
    public override void OnEnter(PlayerController player)
    {
        this._player = player;
        stuckTimer = player.stuckDuration;
        hasHandledJumpInput = false;

        // ��������Ϊ�˶�ѧģʽ����ͣ����ģ�⣩
        player.rb.bodyType = RigidbodyType2D.Kinematic;
        player.rb.gravityScale = 0;
        player.rb.linearVelocity = Vector2.zero;
    }

    // ÿ֡�߼����£�������롢��ʱ��
    public override void LogicUpdate()
    {
        if (_player == null) return;

        // �����Ծ���루���ո��뿪��ס״̬��
        bool jumpPressed = _player.inputControl.Player.Jump.IsPressed();
        if (jumpPressed && !hasHandledJumpInput)
        {
            hasHandledJumpInput = true;
            EscapeStuck();
            return;
        }

        // ��ʱ�Զ��뿪��ס״̬
        stuckTimer -= Time.deltaTime;
        if (stuckTimer <= 0)
        {
            EscapeStuck();
        }
    }

    // �������£����ֽ�ɫλ����ê��ͬ����
    public override void PhysicsUpdate()
    {
        if (_player != null && anchorPoint != null)
        {
            _player.transform.position = anchorPoint.position + positionOffset;
            _player.rb.linearVelocity = Vector2.zero;
        }
    }

    // �뿪��ס״̬���ָ�������ʩ�������л�״̬��
    private void EscapeStuck()
    {
        if (_player == null) return;

        // �ָ�����Ϊ��̬ģʽ����������ģ�⣩
        _player.rb.bodyType = RigidbodyType2D.Dynamic;
        _player.rb.gravityScale = _player.normalGravityScale;

        // ʩ���뿪�����ý�ɫ����/������
        Vector2 escapeVelocity = Vector2.up * _player.jumpForce * 0.4f;
        if (_player.inputDirection.x != 0)
        {
            escapeVelocity.x = _player.inputDirection.x * _player.speed * 0.6f;
        }
        _player.rb.linearVelocity = escapeVelocity;

        // �л�״̬��������ܣ��������䣩
        _player.SwitchState(_player.physicsCheck.isGround ? _player.runState : _player.fallState);
    }

    // �뿪״̬ʱ����
    public override void OnExit(PlayerController player)
    {
        CleanupTempPoint();
        anchorPoint = null;
        hasHandledJumpInput = false;
    }
}