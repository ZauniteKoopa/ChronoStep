using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunningRat : AbstractEnemyBehavior
{
    // Speed at which rat is going
    [SerializeField]
    private float runSpeed = 5f;
    [SerializeField]
    private IBlockerSensor leftBlocker;
    [SerializeField]
    private IBlockerSensor rightBlocker;
    private bool goingLeft = true;

    // Start is called before the first frame update
    protected override void initialize()
    {

        if (leftBlocker == null || rightBlocker == null) {
            Debug.LogError("Blockers not appropriately setup");
        }

        // Start behavior sequence
        StartCoroutine(behaviorSequence());
    }


    // Main function to move this enemy one step in the current direction, for one frame
    protected override void behaviorUpdate() {
        // Move in given direction
        Vector2 moveDir = (goingLeft) ? Vector2.left : Vector2.right;
        transform.Translate(moveDir * runSpeed * Time.fixedDeltaTime, Space.World);

        // Change state based on collision
        if (goingLeft && leftBlocker.isBlocked()) {
            goingLeft = false;
            enemySprite.flipX = true;

        } else if (!goingLeft && rightBlocker.isBlocked()) {
            goingLeft = true;
            enemySprite.flipX = false;
        }
    }
}
