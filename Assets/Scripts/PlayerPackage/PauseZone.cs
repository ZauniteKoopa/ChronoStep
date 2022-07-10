using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseZone : MonoBehaviour
{
    // Create a collection of AbstractProjectiles
    private HashSet<AbstractProjectile> inRangeProjectiles;
    private SpriteRenderer render;
    private readonly object projLock = new object();
    [SerializeField]
    private float pauseDuration = 5f;


    // On awake, create Hashset
    private void Awake() {
        inRangeProjectiles = new HashSet<AbstractProjectile>();
        render = GetComponent<SpriteRenderer>();
        render.enabled = false;
    }

    
    // Public function to pause elements nearby
    public void pause() {
        StartCoroutine(pauseSequence());
    }


    // Pause sequence
    private IEnumerator pauseSequence() {
        // Pause all enemy projectiles
        lock (projLock) {
            foreach (AbstractProjectile proj in inRangeProjectiles) {
                if (proj != null) {
                    proj.pause(pauseDuration);
                }
            }
        }

        render.enabled = true;

        yield return new WaitForSeconds(0.15f);

        render.enabled = false;
    }


    // Event handler for when projectiles go in range
    private void OnTriggerEnter2D(Collider2D collider) {
        AbstractProjectile proj = collider.GetComponent<AbstractProjectile>();

        if (proj != null) {
            lock (projLock) {
                inRangeProjectiles.Add(proj);
            }

            proj.destroyEvent.AddListener(onProjectileDestroyed);
        }
    }


    // Event handler for when projectiles go out of range
    private void OnTriggerExit2D(Collider2D collider) {
        AbstractProjectile proj = collider.GetComponent<AbstractProjectile>();

        if (proj != null) {
            lock (projLock) {
                inRangeProjectiles.Remove(proj);
            }

            proj.destroyEvent.RemoveListener(onProjectileDestroyed);
        }
    }


    // Event handler for when projectiles that are in range get destroyed
    private void onProjectileDestroyed(AbstractProjectile proj) {
        lock (projLock) {
            if (inRangeProjectiles.Contains(proj)) {
                inRangeProjectiles.Remove(proj);
            }
        }

        proj.destroyEvent.RemoveListener(onProjectileDestroyed);
    }
    
}
