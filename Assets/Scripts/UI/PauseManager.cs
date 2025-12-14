using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Root")]
    public GameObject pausePanel;

    [Header("Sub Pages")]
    public GameObject pauseMenuPanel;   // 第一页：Resume/Options/Exit
    public GameObject optionsPanel;     // 第二页：音量/返回

    public bool isPaused = false;

    void Start()
    {
        // 初始都关掉
        if (pausePanel) pausePanel.SetActive(false);
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                OpenPause();
            }
            else
            {
                // 如果正在 Options 页，按 ESC 返回暂停菜单
                if (optionsPanel && optionsPanel.activeSelf)
                    CloseOptions();
                else
                    ClosePause();
            }
        }
    }

    public void OpenPause()
    {
        isPaused = true;
        Time.timeScale = 0;
        if (pausePanel) pausePanel.SetActive(true);

        // 进入暂停默认显示菜单页
        if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);
    }

    public void ClosePause()
    {
        isPaused = false;
        Time.timeScale = 1;
        if (pausePanel) pausePanel.SetActive(false);
    }

    public void Resume()
    {
        ClosePause();
    }

    public void OpenOptions()
    {
        // 仍然保持暂停
        isPaused = true;
        Time.timeScale = 0;

        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel) optionsPanel.SetActive(false);
        if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Opening");
    }
}
