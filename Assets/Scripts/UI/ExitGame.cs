using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void OnExitClicked()
    {
        Debug.Log("ExitGame: quitting application");
        Application.Quit();
    }
}
