using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriAspid : Aspid
{
    // Varialbe to angle
    [SerializeField]
    private float projAngle = 30f;

    // Main function to fire projectile
    protected override void fireProjectile(Transform tgt) {
        // Fire straight line base projectile
        base.fireProjectile(tgt);

        // Get angled directions
        Vector3 originalDirection = (tgt.position - transform.position).normalized;
        Vector3 angleDirection1 = Quaternion.AngleAxis(projAngle, Vector3.forward) * originalDirection;
        Vector3 angleDirection2 = Quaternion.AngleAxis(-projAngle, Vector3.forward) * originalDirection;

        // Create projectile from angle one
        StraightLineProjectile currentProj = Object.Instantiate(turretProjectile, transform.position, Quaternion.identity);
        currentProj.changeDirection(angleDirection1);
        currentProj.startMovement(transform);

        // Create projectile from angle two
        currentProj = Object.Instantiate(turretProjectile, transform.position, Quaternion.identity);
        currentProj.changeDirection(angleDirection2);
        currentProj.startMovement(transform);
    }
}
