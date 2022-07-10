using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerFeet : MonoBehaviour
{
    // Unity events to connect to
    public UnityEvent landingEvent;
    public UnityEvent hitEnemyEvent;
    public UnityEvent fallingEvent;

    // Instance variables
    private int numGround = 0;
    private readonly object groundLock = new object();


    // Main event handler function to collect colliders as they enter the trigger box
    //  Pre: collision layers must be set because this considers all collisions possible
    //  Post: Increments numGround
    private void OnTriggerEnter2D(Collider2D collider) {
        EnemyHurtbox testEnemy = collider.GetComponent<EnemyHurtbox>();
        int colliderLayer = collider.gameObject.layer;

        // Case in which collider is enemy
        if (testEnemy != null) {
            hitEnemyEvent.Invoke();
            testEnemy.hurt(1);

        // Case in which collider is a platform
        } else if (colliderLayer == LayerMask.NameToLayer("SolidEnviornment")){
             // Update ground
            lock(groundLock) {
                // If first time on ground after jumping, land
                if (numGround == 0) {
                    landingEvent.Invoke();
                }

                numGround++;
            }
        }
    }

    // Main event handler function to remove colliders when they exit the trigger box
    //  Pre: collision layers must be set because this considers all collisions possible
    //  Post: Decrements numGround
    private void OnTriggerExit2D(Collider2D collider) {
        int colliderLayer = collider.gameObject.layer;

        if (colliderLayer == LayerMask.NameToLayer("SolidEnviornment")) {
            // Update ground
            lock(groundLock) {
                int prevGround = numGround;
                numGround -= (numGround == 0) ? 0 : 1;

                // If first time on ground after jumping, land
                if (numGround == 0 && prevGround > 0) {
                    fallingEvent.Invoke();
                }
            }
        }
    }
}
