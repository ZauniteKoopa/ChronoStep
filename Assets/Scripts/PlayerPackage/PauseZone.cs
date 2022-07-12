using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseZone : MonoBehaviour
{
    // Create a collection of AbstractProjectiles
    private HashSet<AbstractProjectile> inRangeProjectiles;
    private readonly object projLock = new object();

    private HashSet<EnemyStatus> inRangeEnemies;
    private readonly object enemyLock = new object();

    private SpriteRenderer render;
    [SerializeField]
    private float pauseDuration = 5f;
    [SerializeField]
    private float pauseCooldown = 12f;
    private bool pauseReady = true;

    [SerializeField]
    private PlayerUI playerUI;


    // On awake, create Hashset
    private void Awake() {
        inRangeProjectiles = new HashSet<AbstractProjectile>();
        inRangeEnemies = new HashSet<EnemyStatus>();
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

        // Set up timer
        float timer = 0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
        playerUI.displayPauseCooldownState(0f, pauseCooldown);

        // Timer loop
        while (timer < pauseCooldown) {
            yield return waitFrame;

            timer += Time.fixedDeltaTime;
            playerUI.displayPauseCooldownState(timer, pauseCooldown);
        }

        playerUI.displayPauseCooldownState(timer, pauseCooldown);
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

            // Pause all enemies
            lock (enemyLock) {
                foreach (EnemyStatus enemy in inRangeEnemies) {
                    if (enemy != null) {
                        enemy.pause(pauseDuration);
                    }
                }
            }

            timer += Time.fixedDeltaTime;
        }
        
        render.enabled = false;
    }


    // Event handler for when projectiles go in range
    private void OnTriggerEnter2D(Collider2D collider) {
        AbstractProjectile proj = collider.GetComponent<AbstractProjectile>();
        EnemyStatus enemy = collider.GetComponent<EnemyStatus>();

        // Case for projectiles
        if (proj != null) {
            lock (projLock) {
                inRangeProjectiles.Add(proj);
            }

            proj.destroyEvent.AddListener(onProjectileDestroyed);
        }

        // Case for enemies
        if (enemy != null) {
            lock (enemyLock) {
                inRangeEnemies.Add(enemy);
            }
        }
    }


    // Event handler for when projectiles go out of range
    private void OnTriggerExit2D(Collider2D collider) {
        AbstractProjectile proj = collider.GetComponent<AbstractProjectile>();
        EnemyStatus enemy = collider.GetComponent<EnemyStatus>();

        // Case for projectile
        if (proj != null) {
            lock (projLock) {
                inRangeProjectiles.Remove(proj);
            }

            proj.destroyEvent.RemoveListener(onProjectileDestroyed);
        }

        // Case for enemies
        if (enemy != null) {
            lock (enemyLock) {
                inRangeEnemies.Remove(enemy);
            }
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
