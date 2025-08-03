using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MovingObject : MonoBehaviour
{
    [Header("移动设置")]
    public Transform startPoint;
    public Transform endPoint;
    public float moveSpeed = 2f;
    public bool pingPong = true;

    [Header("伤害设置")]
    public LayerMask playerLayer;
    public Vector2 knockbackForce = new Vector2(3f, 5f);

    private Vector3 targetPosition;
    private bool isMovingToEnd = true;
    private Collider2D objectCollider;

    private void Awake()
    {
        objectCollider = GetComponent<Collider2D>();
        if (objectCollider == null)
        {
            enabled = false;
            return;
        }
        objectCollider.isTrigger = true;
    }

    private void Start()
    {
        if (startPoint != null && endPoint != null)
        {
            transform.position = startPoint.position;
            targetPosition = endPoint.position;
        }
    }

    private void Update()
    {
        MoveBetweenPoints();
    }

    private void MoveBetweenPoints()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f && pingPong)
        {
            isMovingToEnd = !isMovingToEnd;
            targetPosition = isMovingToEnd ? endPoint.position : startPoint.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.isInvincible)
            {
                player.SwitchState(player.hurtState);
                Vector2 dir = (other.transform.position - transform.position).normalized;
                player.rb.linearVelocity = Vector2.zero;
                player.rb.AddForce(new Vector2(dir.x * knockbackForce.x, knockbackForce.y), ForceMode2D.Impulse);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (objectCollider != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            if (objectCollider is BoxCollider2D box)
            {
                Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
            }
            else if (objectCollider is CircleCollider2D circle)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
        }

        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
            Gizmos.DrawSphere(startPoint.position, 0.1f);
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(endPoint.position, 0.1f);
        }
    }
}