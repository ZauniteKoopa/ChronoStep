using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyRoom : MonoBehaviour
{
    // Main variables
    [SerializeField]
    private EnemyStatus[] enemies;

    public UnityEvent enemyRoomTriggerEvent;
    public UnityEvent enemyRoomClearedEvent;
    private int enemiesLeft;
    private readonly object enemyLock = new object();


    // On awake set this up
    private void Awake() {
        if (enemies.Length == 0) {
            Debug.LogError("No enemies assigned to this room.", transform);
        }

        enemiesLeft = enemies.Length;

        foreach (EnemyStatus enemy in enemies) {
            if (enemy == null) {
                Debug.LogError("Null Enemy Found assigned to this room", transform);
            }

            enemy.deathEvent.AddListener(onEnemyDeath);
        }

    }


    // Main event handler for when an enemy dies
    private void onEnemyDeath() {
        lock (enemyLock) {
            enemiesLeft--;

            if (enemiesLeft <= 0) {
                enemyRoomClearedEvent.Invoke();
            }
        }
    }


    // When player enters trigger, start the enemy room
    private void OnTriggerEnter2D(Collider2D collider) {
        PlatformerController2D playerStatus = collider.GetComponent<PlatformerController2D>();

        if (playerStatus != null) {
            GetComponent<Collider2D>().enabled = false;
            enemyRoomTriggerEvent.Invoke();
        }
    }
}
