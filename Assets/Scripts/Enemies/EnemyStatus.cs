using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyStatus : MonoBehaviour
{
    // Health
    [SerializeField]
    private int health = 3;
    [SerializeField]
    private SpriteRenderer sprite;
    private Animator spriteAnimator;
    public UnityEvent deathEvent;
    private Color originalColor;

    // Health locks
    private readonly object healthLock = new object();
    private bool invulnerable = false;
    private float invulnerableDuration = 0.2f;

    // Pause management
    private bool paused = false;
    private bool pauseInvulnerable = false;
    private float pauseInvulnerabilityDuration = 5f;
    [SerializeField]
    private Color pauseColor;
    [SerializeField]
    private EnemyUI enemyUI;

    // Damage display
    private float damageDisplayTime = 0.2f;

    // Collection of body hitboxes that trigger unpause event
    [SerializeField]
    private EnemyBodyHitbox[] enemyBodyHitboxes;
    private Rigidbody2D rb;
    private RigidbodyConstraints2D originalPhysicsConstraints;

    // Audio
    private EnemyAudio enemyAudio;


    // On awake set everything up
    private void Awake() {
        // Error check
        if (sprite == null) {
            Debug.LogError("Not connected to sprite");
        }
        spriteAnimator = sprite.GetComponent<Animator>();

        // listen to body hit event
        foreach (EnemyBodyHitbox hitbox in enemyBodyHitboxes) {
            hitbox.hitPlayerEvent.AddListener(onEnemyBodyHit);
        }

        // get rigidbody and get original constraints
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) {
            Debug.LogError("No rigidbody connected to this enemy!", transform);
        }
        originalPhysicsConstraints = rb.constraints;

        enemyAudio = GetComponent<EnemyAudio>();

        // Get color
        originalColor = sprite.color;
    }


    // Main function to handle damage
    public void damage(int dmg) {
        // Do damage
        lock (healthLock) {
            if (!invulnerable) {
                health -= dmg;
        
                // Unpause if you are paused
                if (paused) {
                    paused = false;
                }

                // check for death condition
                if (health <= 0) {
                    // Play death sound
                    deathEvent.Invoke();
                    if (enemyAudio != null) {
                        enemyAudio.playDeathSound();
                    }
                    
                    // Disable everything
                    for (int i = 0; i < transform.childCount; i++){
                        transform.GetChild(i).gameObject.SetActive(false);
                    }

                } else {
                    StartCoroutine(displayDamage());
                    StartCoroutine(invulnerabilitySequence());
                }
            }
        }
    }

    
    // Main function to show that unit got damaged
    private IEnumerator displayDamage() {
        sprite.color = Color.red;
        yield return new WaitForSeconds(damageDisplayTime);
        sprite.color = (paused) ? Color.blue : originalColor;
    }


    // Main invulnerability sequence
    private IEnumerator invulnerabilitySequence() {
        invulnerable = true;
        yield return new WaitForSeconds(invulnerableDuration);
        invulnerable = false;
    }


    // Main function to pause unit
    public void pause(float duration) {
        if (!paused && !pauseInvulnerable) {
            StartCoroutine(pauseSequence(duration));
        }
    }


    // Main event handler function for when enemy's body hit the player
    private void onEnemyBodyHit() {
        if (paused) {
            paused = false;
        }
    }


    // Main function to do pause invulnerability sequence, for a short amount of time, enemies will be invulnerable to pauses
    //  Pre: enemy just got out of being unpaused
    private IEnumerator pauseInvulnerabilitySequence() {
        // Set pause invulnerability to true
        pauseInvulnerable = true;
        enemyUI.displayPauseInvulnerability(true);
        enemyUI.setPauseInvulnerabilityStatus(0f, pauseInvulnerabilityDuration);

        // Establish timer
        float timer = 0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

        // Timer loop
        while (timer < pauseInvulnerabilityDuration) {
            yield return waitFrame;

            timer += Time.fixedDeltaTime;
            enemyUI.setPauseInvulnerabilityStatus(timer, pauseInvulnerabilityDuration);
        }

        enemyUI.displayPauseInvulnerability(false);
        pauseInvulnerable = false;

    }


    // Main function to go pause sequence 
    private IEnumerator pauseSequence(float duration) {
        Debug.Assert(duration > 1.5f);

        // Set up timer
        paused = true;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        float timer = 0f;
        sprite.color = pauseColor;
        if (spriteAnimator != null) {
            spriteAnimator.speed = 0f;
        }

        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

        // Be blue for a while
        while (timer < (duration - 1.5f) && paused) {
            yield return waitFrame;
            timer += Time.fixedDeltaTime;
        }

        // Start blinking
        float blinkTimer = 0f;
        bool isBlue = true;

        while (timer < duration && paused) {
            yield return waitFrame;

            // Update timers
            timer += Time.fixedDeltaTime;
            blinkTimer += Time.fixedDeltaTime;

            // Check for blink
            if (blinkTimer > 0.1f) {
                sprite.color = (isBlue) ? originalColor : pauseColor;
                isBlue = !isBlue;
                blinkTimer = 0f;
            }
        }

        // Reset
        StartCoroutine(pauseInvulnerabilitySequence());
        rb.constraints = originalPhysicsConstraints;
        sprite.color = originalColor;
        if (spriteAnimator != null) {
            spriteAnimator.speed = 1f;
        }
        paused = false;
    }


    // Main function to check if this unit is paused
    public bool isPaused() {
        return paused;
    }
}
