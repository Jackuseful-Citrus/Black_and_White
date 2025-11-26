using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 开场界面按钮逻辑：Start/Exit。
/// </summary>
public class OpeningUI : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Main";

    public void StartGame()
    {
        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("OpeningUI: gameSceneName 未设置");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
