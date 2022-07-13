using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingTurret : AbstractEnemyBehavior
{
    [Header("Behavior variables")]
    [SerializeField]
    private float fireInterval = 2.5f;
    [SerializeField]
    protected StraightLineProjectile turretProjectile;
    
    private Transform target = null;
    private float fireTimer = 0f;


    // Event handler function for when sensing the player
    protected override void onPlayerSensed(Transform player) {
        target = player;
        StartCoroutine(behaviorSequence());
    }


    // Behavior that's done per frame
    protected override void behaviorUpdate() {
        fireTimer += Time.fixedDeltaTime;

        // If timer went past interval, fire projectile and reset timer
        if (fireTimer >= fireInterval) {
            fireProjectile(target);
            fireTimer = 0f;
        }
    }


    // Main function to fire projectile
    protected virtual void fireProjectile(Transform tgt) {
        StraightLineProjectile currentProj = Object.Instantiate(turretProjectile, transform.position, Quaternion.identity);
        currentProj.changeDirection((tgt.position - transform.position).normalized);
        currentProj.startMovement(transform);
    }
}
