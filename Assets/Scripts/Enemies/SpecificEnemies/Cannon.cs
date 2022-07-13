using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [SerializeField]
    private StraightLineProjectile cannonProj;
    [SerializeField]
    private float fireRate = 3f;
    [SerializeField]
    private Vector2 projectileDir;
    private EnemyStatus status;


    // On Awake, start AI sequence
    private void Awake() {
        status = GetComponent<EnemyStatus>();

        if (status == null) {
            Debug.LogError("No enemy status found on this unit", transform);
        }

        StartCoroutine(fireSequence());
    }

    // Main function to fire constantly
    private IEnumerator fireSequence() {
        while (true) {
            WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
            float timer = 0f;

            while (timer < fireRate) {
                yield return new WaitUntil(() => !status.isPaused());
                timer += Time.fixedDeltaTime;

                yield return waitFrame;
                timer += Time.fixedDeltaTime;
            }

            StraightLineProjectile currentProj = Object.Instantiate(cannonProj, transform.position, Quaternion.identity);
            currentProj.changeDirection(projectileDir);
            currentProj.startMovement(transform);
        }
    }
}
