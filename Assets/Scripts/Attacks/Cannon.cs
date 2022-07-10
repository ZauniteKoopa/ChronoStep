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


    // On Awake, start AI sequence
    private void Awake() {
        StartCoroutine(fireSequence());
    }

    // Main function to fire constantly
    private IEnumerator fireSequence() {
        while (true) {
            yield return new WaitForSeconds(fireRate);

            StraightLineProjectile currentProj = Object.Instantiate(cannonProj, transform.position, Quaternion.identity);
            currentProj.changeDirection(projectileDir);
            currentProj.startMovement(transform);
        }
    }
}
