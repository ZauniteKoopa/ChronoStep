using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBodyHitbox : MonoBehaviour
{
    // Only attack player if on trigger stay indicated
    private void OnTriggerStay2D(Collider2D collider) {
        PlatformerController2D playerStatus = collider.GetComponent<PlatformerController2D>();

        if (playerStatus != null) {
            playerStatus.damage(1, transform.position);
        }
    }
}
