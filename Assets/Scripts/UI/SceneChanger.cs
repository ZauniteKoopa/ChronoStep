using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Main function to change scenes
    public void changeScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    // Main function to quit application
    public void quit() {
        Application.Quit();
    }
}
