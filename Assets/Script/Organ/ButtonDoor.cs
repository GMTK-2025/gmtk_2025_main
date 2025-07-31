using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ButtonDoorController : MonoBehaviour
{
    [Header("门设置")]
    public Transform door;                  // 门物体
    public float doorMoveHeight = 2f;       // 门移动高度
    public float moveSpeed = 5f;            // 移动速度

    [Header("按钮1设置（控制门上移）")]
    public GameObject button1;              // 第一个按钮物体
    public float button1PressDepth = 0.2f;  // 按钮1按下深度

    [Header("按钮2设置（强制控制门下移）")]
    public GameObject button2;              // 第二个按钮物体
    public float button2PressDepth = 0.2f;  // 按钮2按下深度

    private Vector3 originalDoorPosition;   // 门初始位置（关闭位置）
    private Vector3 raisedDoorPosition;     // 门升起位置（打开位置）
    private Vector3 originalButton1Pos;     // 按钮1初始位置
    private Vector3 originalButton2Pos;     // 按钮2初始位置

    private HashSet<Collider2D> button1Objects = new HashSet<Collider2D>();
    private HashSet<Collider2D> button2Objects = new HashSet<Collider2D>();

    private bool isButton1Pressed;
    private bool isButton2Pressed;

    void Start()
    {
        // 初始化位置
        originalDoorPosition = door.position;
        raisedDoorPosition = originalDoorPosition + Vector3.up * doorMoveHeight;
        originalButton1Pos = button1.transform.position;
        originalButton2Pos = button2.transform.position;

        // 自动配置按钮碰撞器和触发
        SetupButtonTrigger(button1, OnButton1Enter, OnButton1Exit);
        SetupButtonTrigger(button2, OnButton2Enter, OnButton2Exit);
    }

    // 自动设置按钮的碰撞器和触发事件
    private void SetupButtonTrigger(GameObject button, System.Action<Collider2D> enterAction, System.Action<Collider2D> exitAction)
    {
        BoxCollider2D collider = button.GetComponent<BoxCollider2D>();
        if (collider == null) collider = button.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        ButtonTriggerHandler handler = button.GetComponent<ButtonTriggerHandler>();
        if (handler == null) handler = button.AddComponent<ButtonTriggerHandler>();
        handler.enterAction = enterAction;
        handler.exitAction = exitAction;
    }

    void Update()
    {
        // 按钮状态检测
        isButton1Pressed = button1Objects.Count > 0;
        isButton2Pressed = button2Objects.Count > 0;

        // button2按下时强制关门，否则由button1控制开门
        Vector3 targetDoorPos;
        if (isButton2Pressed)
        {
            targetDoorPos = originalDoorPosition;
        }
        else
        {
            // button2未按下 → 由button1控制（按下则开门，否则关门）
            targetDoorPos = isButton1Pressed ? raisedDoorPosition : originalDoorPosition;
        }

        // 门移动
        door.position = Vector3.Lerp(door.position, targetDoorPos, moveSpeed * Time.deltaTime);

        // 按钮1移动
        Vector3 targetButton1Pos = isButton1Pressed ?
            originalButton1Pos + Vector3.down * button1PressDepth :
            originalButton1Pos;
        button1.transform.position = Vector3.Lerp(button1.transform.position, targetButton1Pos, moveSpeed * Time.deltaTime);

        // 按钮2移动
        Vector3 targetButton2Pos = isButton2Pressed ?
            originalButton2Pos + Vector3.down * button2PressDepth :
            originalButton2Pos;
        button2.transform.position = Vector3.Lerp(button2.transform.position, targetButton2Pos, moveSpeed * Time.deltaTime);
    }

    // 按钮1触发回调
    private void OnButton1Enter(Collider2D other) => button1Objects.Add(other);
    private void OnButton1Exit(Collider2D other) => button1Objects.Remove(other);

    // 按钮2触发回调
    private void OnButton2Enter(Collider2D other) => button2Objects.Add(other);
    private void OnButton2Exit(Collider2D other) => button2Objects.Remove(other);

    // 处理按钮触发事件
    private class ButtonTriggerHandler : MonoBehaviour
    {
        public System.Action<Collider2D> enterAction;
        public System.Action<Collider2D> exitAction;

        private void OnTriggerEnter2D(Collider2D other) => enterAction?.Invoke(other);
        private void OnTriggerExit2D(Collider2D other) => exitAction?.Invoke(other);
    }
}
