using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    // Main variables
    private bool paused = false;
    private float prevTimeScale = 1f;


    // Main function to pause
    public void pause() {
        if (!paused) {
            prevTimeScale = Time.timeScale;
            gameObject.SetActive(true);
            Time.timeScale = 0f;
            paused = true;
        }
    }


    // Main function to unpause
    public void unpause() {
        if (paused) {
            gameObject.SetActive(false);
            Time.timeScale = prevTimeScale;
            paused = false;
        }
    }


    // Main accessor function to check if you're paused
    public bool menuActive() {
        return paused;
    }


    // Main event handler function: on quit button
    public void quit() {
        Application.Quit();
    }


    // Main event handler function: on go to main menu
    public void goToMainMenu() {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("MainMenu");
    }

}
