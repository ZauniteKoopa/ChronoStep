using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ProjectileDelegate : UnityEvent<AbstractProjectile> {}

public abstract class AbstractProjectile : MonoBehaviour
{
    // Variables to manage pause
    public UnityEvent pausedEvent;
    public UnityEvent unpausedEvent;
    public ProjectileDelegate destroyEvent;
    private bool paused = false;

    // Reference variables
    private Collider2D myCollider;
    private SpriteRenderer myRender;
    private Color trueColor;


    // Main function to initialize variables
    private void Awake() {
        myCollider = GetComponent<Collider2D>();
        myRender = GetComponent<SpriteRenderer>();
        destroyEvent = new ProjectileDelegate();
        trueColor = myRender.color;
    }


    // Main function to set movement
    public void startMovement(Transform target) {
        StartCoroutine(movement(target));
    }


    // Main function to handle movement
    protected abstract IEnumerator movement(Transform target);


    // Main function to check if it's paused, returns true if successful
    public bool isPaused() {
        return paused;
    }


    // Function to create pause sequence
    public void pause(float pauseDuration) {
        if (!paused) {
            StartCoroutine(pauseSequence(pauseDuration));
        }
    }


    // Private IEnumerator for pausing enemies
    private IEnumerator pauseSequence(float duration) {
        Debug.Assert(duration > 1.5f);

        // Pause projectile
        gameObject.layer = LayerMask.NameToLayer("SolidEnviornment");
        paused = true;
        myCollider.isTrigger = false;
        myRender.color = Color.blue;
        pausedEvent.Invoke();

        // Stay blue for a long time
        yield return new WaitForSeconds(duration - 1.5f);

        // Constantly blink every 0.1 seconds
        float timer = 0f;
        bool isBlue = true;
        while (timer < 1.5f) {
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;

            myRender.color = (isBlue) ? trueColor : Color.blue;
            isBlue = !isBlue;
        }

        // Unpause projectile
        gameObject.layer = LayerMask.NameToLayer("EnemyHitbox");
        paused = false;
        myCollider.isTrigger = true;
        myRender.color = trueColor;
        unpausedEvent.Invoke();
    }


    // On trigger 
    private void OnTriggerStay2D(Collider2D collider) {

        // Only collide if this is the player or the enviornment
        int colliderLayer = collider.gameObject.layer;
        if (colliderLayer == LayerMask.NameToLayer("SolidEnviornment") || colliderLayer == LayerMask.NameToLayer("Player")) {

            // If player exists do damage
            PlatformerController2D player = collider.GetComponent<PlatformerController2D>();
            if (player != null) {
                player.damage(1, transform.position);
            }

            // Destroy
            destroyEvent.Invoke(this);
            Object.Destroy(gameObject);
        }
    }

}
