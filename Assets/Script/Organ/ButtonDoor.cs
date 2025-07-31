using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ButtonDoorController : MonoBehaviour
{
    [Header("������")]
    public Transform door;                  // ������
    public float doorMoveHeight = 2f;       // ���ƶ��߶�
    public float moveSpeed = 5f;            // �ƶ��ٶ�

    [Header("��ť1���ã����������ƣ�")]
    public GameObject button1;              // ��һ����ť����
    public float button1PressDepth = 0.2f;  // ��ť1�������

    [Header("��ť2���ã�ǿ�ƿ��������ƣ�")]
    public GameObject button2;              // �ڶ�����ť����
    public float button2PressDepth = 0.2f;  // ��ť2�������

    private Vector3 originalDoorPosition;   // �ų�ʼλ�ã��ر�λ�ã�
    private Vector3 raisedDoorPosition;     // ������λ�ã���λ�ã�
    private Vector3 originalButton1Pos;     // ��ť1��ʼλ��
    private Vector3 originalButton2Pos;     // ��ť2��ʼλ��

    private HashSet<Collider2D> button1Objects = new HashSet<Collider2D>();
    private HashSet<Collider2D> button2Objects = new HashSet<Collider2D>();

    private bool isButton1Pressed;
    private bool isButton2Pressed;

    void Start()
    {
        // ��ʼ��λ��
        originalDoorPosition = door.position;
        raisedDoorPosition = originalDoorPosition + Vector3.up * doorMoveHeight;
        originalButton1Pos = button1.transform.position;
        originalButton2Pos = button2.transform.position;

        // �Զ����ð�ť��ײ���ʹ���
        SetupButtonTrigger(button1, OnButton1Enter, OnButton1Exit);
        SetupButtonTrigger(button2, OnButton2Enter, OnButton2Exit);
    }

    // �Զ����ð�ť����ײ���ʹ����¼�
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
        // ��ť״̬���
        isButton1Pressed = button1Objects.Count > 0;
        isButton2Pressed = button2Objects.Count > 0;

        // button2����ʱǿ�ƹ��ţ�������button1���ƿ���
        Vector3 targetDoorPos;
        if (isButton2Pressed)
        {
            targetDoorPos = originalDoorPosition;
        }
        else
        {
            // button2δ���� �� ��button1���ƣ��������ţ�������ţ�
            targetDoorPos = isButton1Pressed ? raisedDoorPosition : originalDoorPosition;
        }

        // ���ƶ�
        door.position = Vector3.Lerp(door.position, targetDoorPos, moveSpeed * Time.deltaTime);

        // ��ť1�ƶ�
        Vector3 targetButton1Pos = isButton1Pressed ?
            originalButton1Pos + Vector3.down * button1PressDepth :
            originalButton1Pos;
        button1.transform.position = Vector3.Lerp(button1.transform.position, targetButton1Pos, moveSpeed * Time.deltaTime);

        // ��ť2�ƶ�
        Vector3 targetButton2Pos = isButton2Pressed ?
            originalButton2Pos + Vector3.down * button2PressDepth :
            originalButton2Pos;
        button2.transform.position = Vector3.Lerp(button2.transform.position, targetButton2Pos, moveSpeed * Time.deltaTime);
    }

    // ��ť1�����ص�
    private void OnButton1Enter(Collider2D other) => button1Objects.Add(other);
    private void OnButton1Exit(Collider2D other) => button1Objects.Remove(other);

    // ��ť2�����ص�
    private void OnButton2Enter(Collider2D other) => button2Objects.Add(other);
    private void OnButton2Exit(Collider2D other) => button2Objects.Remove(other);

    // ����ť�����¼�
    private class ButtonTriggerHandler : MonoBehaviour
    {
        public System.Action<Collider2D> enterAction;
        public System.Action<Collider2D> exitAction;

        private void OnTriggerEnter2D(Collider2D other) => enterAction?.Invoke(other);
        private void OnTriggerExit2D(Collider2D other) => exitAction?.Invoke(other);
    }
}
