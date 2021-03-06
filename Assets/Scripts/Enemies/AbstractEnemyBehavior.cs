using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractEnemyBehavior : MonoBehaviour
{
    // Main variables
    private EnemyStatus enemyStatus;
    [SerializeField]
    private EnemySensing sensing;
    [SerializeField]
    protected SpriteRenderer enemySprite;

    
    // Initializing elements on awake
    private void Awake() {
        enemyStatus = GetComponent<EnemyStatus>();
        if (enemyStatus == null) {
            Debug.LogError("No enemy status found for this enemy behavior", transform);
        }

        enemyStatus.deathEvent.AddListener(onDeath);

        // Connect to sensing
        if (sensing != null) {
            sensing.sensedPlayerEvent.AddListener(onPlayerSensed);
        }

        initialize();
    }


    // Event handler function for initializing
    protected virtual void initialize() {}


    // Event handler function for when sensing the player
    protected virtual void onPlayerSensed(Transform player) {}


    // Main function to do on behaviorUpdate: this is run on every frame the enemy is active (NOT PAUSED)
    protected abstract void behaviorUpdate();


    // Main function to check if this unit is paused
    protected bool isPaused() {
        return enemyStatus.isPaused();
    }


    // Main event handler function when enemy dies
    private void onDeath() {
        StopAllCoroutines();
    }


    // Main behavior sequence
    protected IEnumerator behaviorSequence() {
        // setup
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

        // main loop
        while (true) {
            // Wait for singular frame
            yield return waitFrame;
            behaviorUpdate();

            // Wait for pausing
            yield return new WaitUntil(() => !enemyStatus.isPaused());
        }
    }


}
