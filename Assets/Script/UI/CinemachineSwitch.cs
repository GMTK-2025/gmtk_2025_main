using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine; // 必须保留

public class CinemachineSwitch : MonoBehaviour
{
    [Header("相机设置")]
    public CinemachineCamera cmMainMenu;   // 主界面相机（新版）
    public CinemachineCamera cmGameplay;   // 游戏场景相机（新版）

    [Header("UI 设置")]
    public Canvas mainMenuCanvas;          // 主界面画布
    public Canvas gameplayCanvas;          // 游戏画布
    public Button startButton;             // 开始按钮
    public Button backButton;              // 返回按钮
    public Button pauseButton;             // 暂停按钮

    [Header("暂停元素")]
    public GameObject pauseElements;       // 暂停时显示的元素

    private bool isPaused = false;
    private Text pauseButtonText;          // 按钮文本

    void Start()
    {
        // 初始化：默认显示主界面
        SwitchToMainMenu();

        // 获取按钮文本
        pauseButtonText = pauseButton.GetComponentInChildren<Text>();
        if (pauseButtonText == null) Debug.LogError("暂停按钮缺少 Text 组件！");

        // 绑定事件
        startButton?.onClick.AddListener(SwitchToGameplay);
        backButton?.onClick.AddListener(SwitchToMainMenu);
        pauseButton?.onClick.AddListener(TogglePause);
    }

    // 切换到主界面（无过渡，直接显示）
    public void SwitchToMainMenu()
    {
        cmMainMenu.Priority = 100;   // 主界面相机优先级最高
        cmGameplay.Priority = 0;     // 游戏相机禁用

        mainMenuCanvas.enabled = true;
        gameplayCanvas.enabled = false;
        pauseElements.SetActive(false);
        Time.timeScale = 1;          // 恢复时间流速
        isPaused = false;
        UpdatePauseButtonText();
    }

    // 切换到游戏场景（无过渡，直接显示）
    public void SwitchToGameplay()
    {
        cmMainMenu.Priority = 0;     // 主界面相机禁用
        cmGameplay.Priority = 100;   // 游戏相机优先级最高

        mainMenuCanvas.enabled = false;
        gameplayCanvas.enabled = true;
        pauseElements.SetActive(false);
        Time.timeScale = 1;          // 恢复时间流速
        isPaused = false;
        UpdatePauseButtonText();
    }

    // 暂停/继续逻辑
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1; // 暂停/恢复时间

        // 显示/隐藏暂停元素
        pauseElements.SetActive(isPaused);
        // 显示/隐藏暂停按钮文本（根据需求可选）
        pauseButtonText.enabled = !isPaused;

        // 更新按钮文本
        pauseButtonText.text = isPaused ? "继续游戏" : "暂停游戏";
    }

    // 强制更新按钮文本（可选）
    private void UpdatePauseButtonText()
    {
        if (pauseButtonText == null) return;
        pauseButtonText.text = isPaused ? "继续游戏" : "暂停游戏";
    }
}