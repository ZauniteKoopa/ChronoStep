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
        AbstractProjectile testProjectile = collider.GetComponent<AbstractProjectile>();
        EnemyHurtbox testEnemy = collider.GetComponent<EnemyHurtbox>();
        int colliderLayer = collider.gameObject.layer;

        // In any abstract projectile, keep track of it
        if (testProjectile != null) {
            connectProjectile(testProjectile);
        }

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
        AbstractProjectile testProjectile = collider.GetComponent<AbstractProjectile>();

        // In any abstract projectile, disconnect
        if (testProjectile != null) {
            disconnectProjectile(testProjectile);
        }

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


    // Main event handler function for when a projectile has been destroyed
    public void onProjectileDestroyed(AbstractProjectile proj) {
        disconnectProjectile(proj);
    }


    // Private helper function to connect to projectile
    private void connectProjectile(AbstractProjectile proj) {
        proj.destroyEvent.AddListener(onProjectileDestroyed);
        proj.pausedEvent.AddListener(onProjectilePaused);
        proj.unpausedEvent.AddListener(onProjectileUnpaused);
    }

    // Private helper function to connect projectile
    private void disconnectProjectile(AbstractProjectile proj) {
        proj.destroyEvent.RemoveListener(onProjectileDestroyed);
        proj.pausedEvent.RemoveListener(onProjectilePaused);
        proj.unpausedEvent.RemoveListener(onProjectileUnpaused);
    }


    // Main event handler for when a projectile has been paused
    public void onProjectilePaused() {

        lock (groundLock) {
            // If first time on ground after jumping, land
            if (numGround == 0) {
                landingEvent.Invoke();
            }

            numGround++;
        }
    }


    // Main event handler function for when projectile is unpaused
    public void onProjectileUnpaused() {
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
