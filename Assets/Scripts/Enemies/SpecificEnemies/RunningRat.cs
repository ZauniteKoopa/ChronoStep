using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunningRat : MonoBehaviour
{
    // Speed at which rat is going
    [SerializeField]
    private float runSpeed = 5f;
    [SerializeField]
    private IBlockerSensor leftBlocker;
    [SerializeField]
    private IBlockerSensor rightBlocker;
    private EnemyStatus enemyStatus;
    private bool goingLeft = true;

    // Start is called before the first frame update
    void Awake()
    {
        enemyStatus = GetComponent<EnemyStatus>();

        if (enemyStatus == null) {
            Debug.LogError("No enemy status found for behavior to reference");
        }

        if (leftBlocker == null || rightBlocker == null) {
            Debug.LogError("Blockers not appropriately setup");
        }

        // Start behavior sequence
        StartCoroutine(behaviorSequence());
    }

    
    // Main AI sequence
    private IEnumerator behaviorSequence() {
        // setup
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

        // main loop
        while (true) {
            // Wait
            yield return waitFrame;
            move();
            yield return new WaitUntil(() => !enemyStatus.isPaused());
            move();
        }
    }

    // Main function to move this enemy one step in the current direction, for one frame
    private void move() {
        // Move in given direction
        Vector2 moveDir = (goingLeft) ? Vector2.left : Vector2.right;
        transform.Translate(moveDir * runSpeed * Time.fixedDeltaTime, Space.World);

        // Change state based on collision
        if (goingLeft && leftBlocker.isBlocked()) {
            goingLeft = false;

        } else if (!goingLeft && rightBlocker.isBlocked()) {
            goingLeft = true;
        }
    }
}
