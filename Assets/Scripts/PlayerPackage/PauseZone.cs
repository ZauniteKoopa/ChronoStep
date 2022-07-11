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
    [SerializeField]
    private float pauseCooldown = 12f;
    private bool pauseReady = true;


    // On awake, create Hashset
    private void Awake() {
        inRangeProjectiles = new HashSet<AbstractProjectile>();
        render = GetComponent<SpriteRenderer>();
        render.enabled = false;
    }


    // Main function to check if you can pause
    public bool canPause() {
        return pauseReady;
    }

    
    // Public function to pause elements nearby
    //  Returns true if successful
    public bool pause() {
        if (pauseReady) {
            StartCoroutine(pauseSequence());
            StartCoroutine(pauseCooldownSequence());
            pauseReady = false;
            return true;
        }

        return false;
    }


    // Pause cooldown sequence
    private IEnumerator pauseCooldownSequence() {
        pauseReady = false;
        yield return new WaitForSeconds(pauseCooldown);
        pauseReady = true;
    }


    // Pause sequence
    private IEnumerator pauseSequence() {

        render.enabled = true;

        float timer = 0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
        while (timer < 0.15f) {
            yield return waitFrame;
            // Pause all enemy projectiles
            lock (projLock) {
                foreach (AbstractProjectile proj in inRangeProjectiles) {
                    if (proj != null) {
                        proj.pause(pauseDuration);
                    }
                }
            }

            timer += Time.fixedDeltaTime;
        }
        //yield return new WaitForSeconds(0.15f);
        
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
