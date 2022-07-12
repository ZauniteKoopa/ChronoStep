using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyBodyHitbox : MonoBehaviour
{
    public UnityEvent hitPlayerEvent;

    // Only attack player if on trigger stay indicated
    private void OnTriggerStay2D(Collider2D collider) {
        PlatformerController2D playerStatus = collider.GetComponent<PlatformerController2D>();

        if (playerStatus != null) {
            bool inflictedDmg = playerStatus.damage(1, transform.position);

            // If successfully hit damage, invoke event
            if (inflictedDmg) {
                hitPlayerEvent.Invoke();
            }
        }
    }
}
