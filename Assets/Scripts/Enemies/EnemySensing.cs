using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EnemySenseDelegate : UnityEvent<Transform> {}

public class EnemySensing : MonoBehaviour
{
    public EnemySenseDelegate onSensedPlayer;
    private bool sensedPlayer = false;


    // Main trigger event function
    private void OnTriggerEnter2D(Collider2D collider) {
        PlatformerController2D testPlayer = collider.GetComponent<PlatformerController2D>();

        if (testPlayer != null && !sensedPlayer) {
            sensedPlayer = true;
            onSensedPlayer.Invoke(testPlayer.transform);
        }
    }
}
