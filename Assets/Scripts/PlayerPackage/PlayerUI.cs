using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private Image pauseCooldownBar;
    [SerializeField]
    private Image[] healthBars;


    // Main function to display pause cooldown
    public void displayPauseCooldownState(float curCooldown, float total) {
        Debug.Assert(total > 0f);

        pauseCooldownBar.fillAmount = curCooldown / total;
    }


    // Main function to display health state
    public void displayHealth(int curHealth) {
        Debug.Assert(curHealth <= healthBars.Length && curHealth >= 0);

        // Enable the health bars that represent current health
        for (int i = 0; i < curHealth; i++) {
            healthBars[i].gameObject.SetActive(true);
        }

        // Disable the ones that are lost
        for (int i = curHealth; i < healthBars.Length; i++) {
            healthBars[i].gameObject.SetActive(false);
        }
    }



}
