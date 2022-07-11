using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformerController2D : MonoBehaviour
{
    // Private Instance Variables
    [Header("Movement Variables")]
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private IBlockerSensor leftBlockerSensor;
    [SerializeField]
    private IBlockerSensor rightBlockerSensor;
    private Vector2 movementVector;
    bool isMoving = false;

    [Header("Jumping variables")]
    private Rigidbody2D rb;
    [SerializeField]
    private float jumpForce = 500f;
    [SerializeField]
    private float wallJumpForce = 400f;
    [SerializeField]
    private float enemyJumpForce = 500f;
    [SerializeField]
    private float gravityScale = 1f;
    [SerializeField]
    private int startingJumps = 1;
    [SerializeField]
    private int tapJumpFrameCheck = 12;
    [SerializeField]
    private float tapJumpCancelTime = 0.35f;
    [SerializeField]
    private float wallJumpHorizontalForceTime = 0.25f;
    private int jumpsLeft;
    private bool inAir = true;
    private bool jumpPressed = false;
    private Coroutine jumpCheckSequence;
    private Coroutine wallJumpSequence;

    [Header("Wall slide variables")]
    [SerializeField]
    private float maxSlideDownSpeed = 5f;
    private bool wallSliding = false;

    [Header("Pause Ability")]
    [SerializeField]
    private PauseZone pauseZone;


    [Header("Health Variables")]
    [SerializeField]
    private int maxHealth = 4;
    [SerializeField]
    private float invincibilityFrameDuration = 1.5f;
    [SerializeField]
    private float damageKnockback = 200f;
    private bool invincible = false;
    private int curHealth;


    // On awake get rigidbody
    private void Awake() {
        // Initialize variables
        jumpsLeft = startingJumps;
        curHealth = maxHealth;

        // Initialize rigidbody
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) {
            Debug.LogError("No rigidbody found on this player, how do I jump?");
        }

        rb.gravityScale = gravityScale;

        // Check if sensors are set
        if (leftBlockerSensor == null || rightBlockerSensor == null) {
            Debug.LogError("Not connected to sensors, check left and right blocker sensors");
        }
    }


    // Main function for doing damage to player
    //  Pre: dmg >= 0f
    //  player is inflicted by this amount of damage
    public void damage(int dmg, Vector2 dmgPosition) {
        if (!invincible) {
            // Decrement health
            curHealth -= dmg;
            Debug.Log("health left: " + curHealth);

            // Apply knockback
            Vector2 playerPosition = transform.position;
            Vector2 damageKnockbackDir = (playerPosition - dmgPosition).normalized;
            rb.velocity = Vector3.zero;
            rb.AddForce(damageKnockback * damageKnockbackDir);

            // Do I-frame sequence if alive, death sequence if dead
            if (curHealth > 0) {
                StartCoroutine(invincibilityFrameSequence());
            } else {
                invincible = true;
                Debug.Log("I'm dead!");
            }
        }
    }


    // Function to do I-frame sequence
    private IEnumerator invincibilityFrameSequence() {
        invincible = true;
        yield return new WaitForSeconds(invincibilityFrameDuration);
        invincible = false;
    }


    // Main function for handling movement: runs every frame
    private void FixedUpdate() {
        // Only do movement if you're moving
        if (isMoving) {
            // Modify movement vector
            Vector2 actualMove = movementVector;
            actualMove.x = (leftBlockerSensor.isBlocked() && actualMove.x < -0.1f) ? 0f : actualMove.x;
            actualMove.x = (rightBlockerSensor.isBlocked() && actualMove.x > 0.1f) ? 0f : actualMove.x;

            // Do translation
            transform.Translate(actualMove * speed * Time.fixedDeltaTime, Space.World);
        }

        // If you're wall sliding, restrict vertical velocity
        checkWallSlide(movementVector.x);
        if (wallSliding) {
            float yVelocity = Mathf.Max(-1 * maxSlideDownSpeed, rb.velocity.y);
            rb.velocity = Vector3.up * yVelocity;
        }
    }


    // Event handler for when movement has changed
    public void onMovementChange(InputAction.CallbackContext value) {
        // Set flag for whether unit is moving
        isMoving = !value.canceled;

        // Set movement vector
        float eventValue = value.ReadValue<float>();
        movementVector = eventValue * Vector2.right;
    }


    // Private helper function to check wall slide
    //  Pre: movementDir is a float indicating movement. -1 to left, 1 to right
    //  Post: sets wallSliding flag and make any noticable changes to rigidbody
    public void checkWallSlide(float movementDir) {
         // Check if wall slide conditions
        bool prevWallSliding = wallSliding;
        bool leftWallSlide = movementDir < -0.0001 && leftBlockerSensor.isBlocked();
        bool rightWallSlide = movementDir > 0.0001 && rightBlockerSensor.isBlocked();

        // Wall sliding occurs when you're leaning to a wall, you're in the air, and you're not in a wall jump sequence
        wallSliding = (leftWallSlide || rightWallSlide) && inAir && wallJumpSequence == null;

        // First frame of wall sliding
        if (wallSliding && !prevWallSliding) {
            Debug.Log("first frame wall sliding");
            rb.velocity = Vector3.zero;
        }
    }


    // Event handler for jumping
    public void onJumpPress(InputAction.CallbackContext value) {
        if (value.started) {
            // Test normal jump
            if (jumpsLeft > 0) {
                // Apply jump
                rb.AddForce(jumpForce * Vector2.up);
                jumpsLeft--;

                // Set jumpPressed flag and check for tap jump
                jumpPressed = true;
                if (jumpCheckSequence != null) {
                    StopCoroutine(jumpCheckSequence);
                }

                jumpCheckSequence = StartCoroutine(checkJumpHeight());
            } else {
                executeWallJump();
            }
            

        } else if (value.canceled) {
            jumpPressed = false;
        }
    }


    // Jump check sequence to check for tap jump
    private IEnumerator checkJumpHeight() {
        // Wait for a certain amount of frames
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
        for (int i = 0; i < tapJumpFrameCheck; i++) {
            yield return waitFrame;
        }

        // If you aren't holding jump anymore, you tap jumpped. Cancel jump
        bool tapJump = !jumpPressed;

        // Wait for appropriate jump height for canceling
        yield return new WaitForSeconds(tapJumpCancelTime);

        // Cancel jump
        if (tapJump) {
            rb.velocity *= -0.5f;
        }

        jumpCheckSequence = null;
    }


    // Main private helper function to check for wall jumps
    //  Pre: none
    //  Post: execute wall jump if you are moving towards a wall
    private void executeWallJump() {
        // You can only execute wall jump if you are in the air
        if (inAir) {

            // Check left wall
            if (leftBlockerSensor.isBlocked() && movementVector.x < -0.1f) {
                Vector2 wallJumpVector = new Vector2(1f, 1f);

                if (wallJumpSequence != null) {
                    StopCoroutine(wallJumpSequence);
                }
                wallJumpSequence = StartCoroutine(wallJumpExecution(wallJumpVector));
            }

            // Check right wall
            else if (rightBlockerSensor.isBlocked() && movementVector.x > 0.1f) {
                Vector2 wallJumpVector = new Vector2(-1f, 1f);

                if (wallJumpSequence != null) {
                    StopCoroutine(wallJumpSequence);
                }
                wallJumpSequence = StartCoroutine(wallJumpExecution(wallJumpVector));
            }
        }
    }


    // Main wall jump sequence IEnumerator
    //  Pre: jumpForceDirection is the direction of the wall jump
    //  Post: Jump away from the vertical wall
    private IEnumerator wallJumpExecution(Vector2 jumpForceDirection) {
        // Apply wall jump
        jumpForceDirection = jumpForceDirection.normalized;
        rb.velocity = Vector3.zero;
        rb.AddForce(wallJumpForce * jumpForceDirection);

        // If you're still checking for tap jump, stop sequence because you're in another jump
        if (jumpCheckSequence != null) {
            StopCoroutine(jumpCheckSequence);
        }

        // Wait for a few seconds before stopping horizontal jump velocity
        yield return new WaitForSeconds(wallJumpHorizontalForceTime);
        rb.velocity = new Vector3(0f, rb.velocity.y);
        wallJumpSequence = null;
    }


    // Event handler for when using the pause ability
    public void onAbilityPress(InputAction.CallbackContext value) {
        if (value.started) {
            rb.velocity = Vector2.zero;
            pauseZone.pause();
        }
    }


    // Event handler for when player has landed on the ground
    public void onLanding() {
        rb.velocity = Vector3.zero;
        jumpsLeft = startingJumps;
        inAir = false;
        wallSliding = false;
    }


    // Event handler for when player is falling
    public void onFalling() {
        jumpsLeft = startingJumps - 1;
        inAir = true;
    }

    // Event handler for when player has stepped on an enemy
    public void onHitEnemy() {
        rb.velocity = Vector3.zero;
        rb.AddForce(enemyJumpForce * Vector3.up);
    }
}
