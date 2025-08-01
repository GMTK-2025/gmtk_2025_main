using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine; // ���뱣��

public class CinemachineSwitch : MonoBehaviour
{
    [Header("�������")]
    public CinemachineCamera cmMainMenu;   // ������������°棩
    public CinemachineCamera cmGameplay;   // ��Ϸ����������°棩

    [Header("UI ����")]
    public Canvas mainMenuCanvas;          // �����滭��
    public Canvas gameplayCanvas;          // ��Ϸ����
    public Button startButton;             // ��ʼ��ť
    public Button backButton;              // ���ذ�ť
    public Button pauseButton;             // ��ͣ��ť

    [Header("��ͣԪ��")]
    public GameObject pauseElements;       // ��ͣʱ��ʾ��Ԫ��

    private bool isPaused = false;
    private Text pauseButtonText;          // ��ť�ı�

    void Start()
    {
        // ��ʼ����Ĭ����ʾ������
        SwitchToMainMenu();

        // ��ȡ��ť�ı�
        pauseButtonText = pauseButton.GetComponentInChildren<Text>();
        if (pauseButtonText == null) Debug.LogError("��ͣ��ťȱ�� Text �����");

        // ���¼�
        startButton?.onClick.AddListener(SwitchToGameplay);
        backButton?.onClick.AddListener(SwitchToMainMenu);
        pauseButton?.onClick.AddListener(TogglePause);
    }

    // �л��������棨�޹��ɣ�ֱ����ʾ��
    public void SwitchToMainMenu()
    {
        cmMainMenu.Priority = 100;   // ������������ȼ����
        cmGameplay.Priority = 0;     // ��Ϸ�������

        mainMenuCanvas.enabled = true;
        gameplayCanvas.enabled = false;
        pauseElements.SetActive(false);
        Time.timeScale = 1;          // �ָ�ʱ������
        isPaused = false;
        UpdatePauseButtonText();
    }

    // �л�����Ϸ�������޹��ɣ�ֱ����ʾ��
    public void SwitchToGameplay()
    {
        cmMainMenu.Priority = 0;     // �������������
        cmGameplay.Priority = 100;   // ��Ϸ������ȼ����

        mainMenuCanvas.enabled = false;
        gameplayCanvas.enabled = true;
        pauseElements.SetActive(false);
        Time.timeScale = 1;          // �ָ�ʱ������
        isPaused = false;
        UpdatePauseButtonText();
    }

    // ��ͣ/�����߼�
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1; // ��ͣ/�ָ�ʱ��

        // ��ʾ/������ͣԪ��
        pauseElements.SetActive(isPaused);
        // ��ʾ/������ͣ��ť�ı������������ѡ��
        pauseButtonText.enabled = !isPaused;

        // ���°�ť�ı�
        pauseButtonText.text = isPaused ? "������Ϸ" : "��ͣ��Ϸ";
    }

    // ǿ�Ƹ��°�ť�ı�����ѡ��
    private void UpdatePauseButtonText()
    {
        if (pauseButtonText == null) return;
        pauseButtonText.text = isPaused ? "������Ϸ" : "��ͣ��Ϸ";
    }
}