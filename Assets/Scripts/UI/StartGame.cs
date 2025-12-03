using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    [SerializeField] private string sceneName = "GreyMap";

    public void OnStartClicked()
    {
        SceneManager.LoadScene(sceneName);
    }
}
