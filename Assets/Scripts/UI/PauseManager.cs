using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;

public class PauseManager : MonoBehaviour

{
    public GameObject pausePanel;  
    public bool isPaused = false;  
  

    void Update()
    {
    
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();  
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused; 

        if (isPaused)
        {
            Time.timeScale = 0; 
            pausePanel.SetActive(true);

        }
        else
        {
            Time.timeScale = 1; 
            pausePanel.SetActive(false); 
        }
    }
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1; 
        SceneManager.LoadScene("Opening"); 
    }

    public void Resume()
    {
        TogglePause();
    }

}
