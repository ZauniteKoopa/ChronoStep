using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractEnemyBehavior : MonoBehaviour
{
    // Main variables
    private EnemyStatus enemyStatus;
    [SerializeField]
    private EnemySensing sensing;

    
    // Initializing elements on awake
    private void Awake() {
        enemyStatus = GetComponent<EnemyStatus>();
        if (enemyStatus == null) {
            Debug.LogError("No enemy status found for this enemy behavior", transform);
        }

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
            behaviorUpdate();
        }
    }


}
