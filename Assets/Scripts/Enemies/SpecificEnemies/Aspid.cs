using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aspid : AbstractEnemyBehavior
{
    [Header("Behavior variables")]
    [SerializeField]
    private float fireInterval = 2.5f;
    [SerializeField]
    protected StraightLineProjectile turretProjectile;
    [SerializeField]
    private float springMovementConstant = 250f;
    [SerializeField]
    private float maxSpeed = 4f;
    [SerializeField]
    private float optimalDistance = 4.5f;
    [SerializeField]
    private float maxUpwardAngle = 80f;

    private Rigidbody2D rb;
    private Transform target = null;
    private float fireTimer = 0f;

    protected override void initialize() {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null) {
            Debug.LogError("No rigidbody found");
        }
    }

    // Event handler function for when sensing the player
    protected override void onPlayerSensed(Transform player) {
        target = player;
        StartCoroutine(behaviorSequence());
    }


    // Behavior that's done per frame
    protected override void behaviorUpdate() {
        // Get appropriate direction that will avoid being on the ground (aspid always wants to be ABOVE player). Calculate force based on distance to target
        Vector2 forceDir = target.position - transform.position;
        float springForce = springMovementConstant * (forceDir.magnitude - optimalDistance);

        if (Vector2.Angle(Vector2.down, forceDir) > maxUpwardAngle) {
            float optimalAngle = (Vector2.Angle(forceDir, Vector2.left) < Vector2.Angle(forceDir, Vector2.right)) ? -1 * maxUpwardAngle : maxUpwardAngle;
            Vector2 optimalPosition = target.position + optimalDistance * -1f * (Quaternion.AngleAxis(optimalAngle, Vector3.forward) * Vector3.down);
            forceDir = transform.position;
            forceDir = optimalPosition - forceDir;

            springForce = Mathf.Abs(springForce);
        }
        
        // Execute movement based on distance towards target
        rb.AddForce(springForce * forceDir.normalized);

        // If velocity going over maxSpeed, set it to max speed
        if (rb.velocity.magnitude > maxSpeed) {
            rb.velocity = (rb.velocity.normalized * maxSpeed);
        }

        // Update fire timer
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
