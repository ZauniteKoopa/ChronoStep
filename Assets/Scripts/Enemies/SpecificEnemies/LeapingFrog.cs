using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeapingFrog : AbstractEnemyBehavior
{
    [Header("Behavior variables")]
    [SerializeField]
    private float jumpForce = 400f;
    [SerializeField]
    private float jumpIntervalTime = 1.5f;
    [SerializeField]
    private float jumpMoveSpeed = 2f;
    private bool jumping = false;
    private float intervalTimer = 0f;
    private Vector2 currentMoveDir = Vector2.left;

    [Header("Collision sensing")]
    [SerializeField]
    private IBlockerSensor footSensor;
    [SerializeField]
    private IBlockerSensor leftSensor;
    [SerializeField]
    private IBlockerSensor rightSensor;

    private Rigidbody2D rb;
    private Transform target;
    private Animator spriteAnimator;


    // On initialization, set up rigidbody
    protected override void initialize() {
        rb = GetComponent<Rigidbody2D>();
        spriteAnimator = enemySprite.GetComponent<Animator>();

        if (rb == null) {
            Debug.LogError("No rigidbody found for this enemy", transform);
        }
    }


    // Event handler function for when sensing the player
    protected override void onPlayerSensed(Transform player) {
        target = player;
        StartCoroutine(behaviorSequence());
    }


    // Main function to handle behavior update
    protected override void behaviorUpdate() {
        // If you're in the process of jumping, move to the left. Only consider yourself grounded if you're going downwards
        if (jumping) {
            // Movement
            bool canMove = (currentMoveDir == Vector2.left && !leftSensor.isBlocked()) || (currentMoveDir == Vector2.right && !rightSensor.isBlocked());
            if (canMove) {
                transform.Translate(currentMoveDir * jumpMoveSpeed * Time.fixedDeltaTime);
            }

            // Check if you've landed
            if (rb.velocity.y <= 0f && footSensor.isBlocked()) {
                jumping = false;
            }

        // In the case where you're not jumping, just wait until it's time to jump again
        } else {
            intervalTimer += Time.fixedDeltaTime;

            // Check if interval timer has passed
            if (intervalTimer >= jumpIntervalTime && footSensor.isBlocked()) {
                rb.AddForce(Vector3.up * jumpForce);
                intervalTimer = 0f;
                jumping = true;
                currentMoveDir = (target.position.x < transform.position.x) ? Vector2.left : Vector2.right;
                enemySprite.flipX = (target.position.x >= transform.position.x);
            }
        }

        // Set animator 
        if (!isPaused()) {
            spriteAnimator.SetFloat("VerticalSpeed", rb.velocity.y);
        }
    }

}
