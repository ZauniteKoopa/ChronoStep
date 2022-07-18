using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGoal : MonoBehaviour
{
    // On trigger enter by the player, heal the player
    private void OnTriggerEnter2D(Collider2D collider) {
        PlatformerController2D testPlayer = collider.GetComponent<PlatformerController2D>();

        if (testPlayer != null) {
            SceneManager.LoadScene("WinScreen");
            Object.Destroy(gameObject);
        }
    }
}
