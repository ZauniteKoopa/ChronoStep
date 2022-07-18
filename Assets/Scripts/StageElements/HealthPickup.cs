using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    // Instance variables
    [SerializeField]
    private int amountHealed = 2;

    // On trigger enter by the player, heal the player
    private void OnTriggerEnter2D(Collider2D collider) {
        PlatformerController2D testPlayer = collider.GetComponent<PlatformerController2D>();

        if (testPlayer != null) {
            testPlayer.heal(amountHealed);
            Object.Destroy(gameObject);
        }
    }
}
