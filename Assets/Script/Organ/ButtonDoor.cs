using System.Collections.Generic;
using UnityEngine;

// 移除自动添加BoxCollider2D的特性，避免给脚本挂载物体自动添加碰撞器
public class ButtonDoorController : MonoBehaviour
{
    [Header("门设置")]
    public Transform door;                  // 门物体
    public float doorMoveHeight = 2f;       // 门移动高度
    public float moveSpeed = 5f;            // 移动速度

    [Header("按钮1设置（控制门上移）")]
    public GameObject button1;              // 第一个按钮物体
    public float button1PressDepth = 0.2f;  // 按钮1按下深度
    [Tooltip("手动指定按钮1的触发器碰撞器（必须提前添加并勾选Is Trigger）")]
    public BoxCollider2D button1Collider;   // 手动配置的按钮1碰撞器

    [Header("按钮2设置（强制控制门下移）")]
    public GameObject button2;              // 第二个按钮物体
    public float button2PressDepth = 0.2f;  // 按钮2按下深度
    [Tooltip("手动指定按钮2的触发器碰撞器（必须提前添加并勾选Is Trigger）")]
    public BoxCollider2D button2Collider;   // 手动配置的按钮2碰撞器

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

        // 校验手动配置的碰撞器
        if (!ValidateButtonColliders())
        {
            enabled = false; // 配置错误时禁用脚本
            return;
        }

        // 仅配置触发事件（不添加碰撞器）
        SetupButtonTrigger(button1, button1Collider, OnButton1Enter, OnButton1Exit);
        SetupButtonTrigger(button2, button2Collider, OnButton2Enter, OnButton2Exit);
    }

    // 校验手动配置的碰撞器是否有效
    private bool ValidateButtonColliders()
    {
        bool isValid = true;

        // 校验按钮1碰撞器
        if (button1Collider == null)
        {
            isValid = false;
        }
        else if (button1Collider.gameObject != button1)
        {
            isValid = false;
        }
        else if (!button1Collider.isTrigger)
        {
            button1Collider.isTrigger = true; // 自动修正
            isValid = false;
        }

        // 校验按钮2碰撞器
        if (button2Collider == null)
        {
            isValid = false;
        }
        else if (button2Collider.gameObject != button2)
        {
            isValid = false;
        }
        else if (!button2Collider.isTrigger)
        {
            button2Collider.isTrigger = true; // 自动修正
            isValid = false;
        }

        return isValid;
    }

    // 仅配置触发事件（不添加碰撞器）
    private void SetupButtonTrigger(GameObject button, BoxCollider2D collider, System.Action<Collider2D> enterAction, System.Action<Collider2D> exitAction)
    {
        // 移除旧的事件处理器，避免重复
        ButtonTriggerHandler oldHandler = button.GetComponent<ButtonTriggerHandler>();
        if (oldHandler != null)
        {
            Destroy(oldHandler);
        }

        // 添加新的事件处理器
        ButtonTriggerHandler handler = button.AddComponent<ButtonTriggerHandler>();
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