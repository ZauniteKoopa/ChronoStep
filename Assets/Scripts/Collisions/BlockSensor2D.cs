using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSensor2D : IBlockerSensor
{
    // Variables to update number of walls touching
    private int numWallsTouched = 0;
    private readonly object numWallsLock = new object();


    // Main function to check sensor
    //  returns true is numWallsTocuhed > 0
    public override bool isBlocked() {
        bool blocked = false;

        lock(numWallsLock) {
            blocked = numWallsTouched > 0;
        }

        return blocked;
    }


    // Main event handler function to collect colliders as they enter the trigger box
    //  Pre: collision layers must be set because this considers all collisions possible
    //  Post: Increments numWallsTouched
    private void OnTriggerEnter2D(Collider2D collider) {
        int colliderLayer = collider.gameObject.layer;
        AbstractProjectile testProjectile = collider.GetComponent<AbstractProjectile>();

        // In any abstract projectile, keep track of it
        if (testProjectile != null) {
            connectProjectile(testProjectile);
        }

        // Case if you hit the enviornment
        if (colliderLayer == LayerMask.NameToLayer("SolidEnviornment")){
            lock(numWallsLock) {
                numWallsTouched++;
            }
        }
    }

    // Main event handler function to remove colliders when they exit the trigger box
    //  Pre: collision layers must be set because this considers all collisions possible
    //  Post: Decrements numWallsTouched
    private void OnTriggerExit2D(Collider2D collider) {
        int colliderLayer = collider.gameObject.layer;
        AbstractProjectile testProjectile = collider.GetComponent<AbstractProjectile>();

        // In any abstract projectile, keep track of it
        if (testProjectile != null) {
            disconnectProjectile(testProjectile);
        }

        // Case if you hit the enviornment
        if (colliderLayer == LayerMask.NameToLayer("SolidEnviornment")){
            lock(numWallsLock) {
                numWallsTouched -= (numWallsTouched == 0) ? 0 : 1;
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
        lock(numWallsLock) {
            numWallsTouched++;
        }
    }


    // Main event handler function for when projectile is unpaused
    public void onProjectileUnpaused() {
        lock(numWallsLock) {
            numWallsTouched -= (numWallsTouched == 0) ? 0 : 1;
        }
    }
}
