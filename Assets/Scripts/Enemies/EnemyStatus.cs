using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyStatus : MonoBehaviour
{
    [SerializeField]
    private int health = 3;
    public UnityEvent deathEvent;


    // Main function to handle damage
    public void damage(int dmg) {
        health-= dmg;

        // check for death condition
        if (health <= 0) {
            deathEvent.Invoke();
            gameObject.SetActive(false);
        }
    }
}
