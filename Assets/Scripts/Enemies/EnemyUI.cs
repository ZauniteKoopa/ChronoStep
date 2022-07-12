using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    [SerializeField]
    private Image pauseInvulnerabilityBar;
    [SerializeField]
    private GameObject pauseInvulnerabilityDisplay;


    // Main function to set status of pauseInvulnerabilityBar
    public void displayPauseInvulnerability(bool displayed) {
        pauseInvulnerabilityDisplay.SetActive(displayed);
    }


    // Main function to set pauseInvulnerability status
    public void setPauseInvulnerabilityStatus(float timePassed, float duration) {
        pauseInvulnerabilityBar.fillAmount = (duration - timePassed) / duration;
    }
}
