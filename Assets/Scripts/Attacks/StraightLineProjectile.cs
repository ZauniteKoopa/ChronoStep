using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightLineProjectile : AbstractProjectile
{
    private const float TIME_OUT_DISTANCE = 30f;
    [SerializeField]
    private float bulletSpeed = 5f;
    private Vector2 bulletDir;

    // Main function to change bulletDir
    public void changeDirection(Vector2 dir) {
        bulletDir = dir;
    }

    // Main function to handle movement
    protected override IEnumerator movement(Transform target)
    {
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
        float curDistance = 0f;
        Vector2 projDir = bulletDir.normalized;

        while (curDistance < TIME_OUT_DISTANCE) {
            // Wait until youre not paused
            yield return new WaitUntil(() => !isPaused());

            // Wait for fixed frame
            yield return waitFrame;

            // Move projectile
            transform.Translate(bulletSpeed * Time.fixedDeltaTime * projDir, Space.World);
            curDistance += (bulletSpeed * Time.fixedDeltaTime);
        }

        Object.Destroy(gameObject);
    }
}
