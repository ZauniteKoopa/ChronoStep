using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    private float initialJumpForce = 300f;
    [SerializeField]
    private float extendedJumpForce = 200f;
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
    private float wallJumpHorizontalForceTime = 0.25f;
    private int jumpsLeft;
    private bool inAir = true;
    private bool jumpPressed = false;
    private bool enemyJumped = false;
    private Coroutine jumpCheckSequence;
    private Coroutine wallJumpSequence;

    // Jump locks
    //private readonly object enemyJumpLock = new object();

    [Header("Wall slide variables")]
    [SerializeField]
    private float maxSlideDownSpeed = 5f;
    private bool wallSliding = false;

    [Header("Pause Ability")]
    [SerializeField]
    private PauseZone pauseZone;
    [SerializeField]
    private float pauseAirJumpForce = 150f;

    [Header("Dash Ability")]
    [SerializeField]
    private float maxDashDistance = 3f;
    [SerializeField]
    private float dashSpeed = 5f;
    [SerializeField]
    private float dashCooldown = 0.35f;
    private bool canDash = true;
    private bool dashing = false;

    [Header("Health Variables")]
    [SerializeField]
    private int maxHealth = 4;
    [SerializeField]
    private float invincibilityFrameDuration = 1.5f;
    [SerializeField]
    private float damageKnockback = 200f;
    private bool invincible = false;
    private int curHealth;


    [Header("Animation")]
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private SpriteRenderer spriteRender;
    [SerializeField]
    private Color hurtColor;

    [Header("UI")]
    [SerializeField]
    private PlayerUI playerUI;
    [SerializeField]
    private PauseMenu pauseMenu;

    // Audio
    private PlayerAudio playerAudio;


    // On awake get rigidbody
    private void Awake() {
        // Initialize variables
        jumpsLeft = startingJumps;
        curHealth = maxHealth;
        playerUI.displayHealth(curHealth);

        // Initialize rigidbody
        rb = GetComponent<Rigidbody2D>();
        playerAudio = GetComponent<PlayerAudio>();
        if (rb == null) {
            Debug.LogError("No rigidbody found on this player, how do I jump?");
        }

        if (pauseMenu == null) {
            Debug.LogError("No pause menu connected to this player");
        }

        if (playerAudio == null) {
            Debug.LogError("No player audio connected to this player");
        }

        rb.gravityScale = gravityScale;

        // Check if sensors are set
        if (leftBlockerSensor == null || rightBlockerSensor == null) {
            Debug.LogError("Not connected to sensors, check left and right blocker sensors");
        }
    }


    // Main function to heal unit
    //  Pre: dmgHealed > 0
    public void heal(int dmgHealed) {
        playerAudio.playHealSound();
        curHealth = Mathf.Min(maxHealth, curHealth + dmgHealed);
        playerUI.displayHealth(curHealth);
    }


    // Main function for doing damage to player
    //  Pre: dmg >= 0f
    //  player is inflicted by this amount of damage, returns if successful
    public bool damage(int dmg, Vector2 dmgPosition) {
        if (!invincible) {
            // Decrement health
            curHealth -= dmg;
            playerUI.displayHealth(curHealth);
            playerAudio.playDamageSound();
            cancelDash();

            // Apply knockback
            Vector2 playerPosition = transform.position;
            Vector2 damageKnockbackDir = (playerPosition - dmgPosition).normalized;
            rb.velocity = Vector3.zero;
            StartCoroutine(addKnockbackForce(damageKnockback * damageKnockbackDir));

            // Do I-frame sequence if alive, death sequence if dead
            if (curHealth > 0) {
                StartCoroutine(invincibilityFrameSequence());
            } else {
                StartCoroutine(death());
            }

            return true;
        }

        return false;
    }

    // Private IEnumerator to add knockback force
    private IEnumerator addKnockbackForce(Vector3 knockback) {
        rb.AddForce(knockback);
        yield return new WaitForSeconds(0.2f);
        rb.velocity = Vector2.zero;
    }


    // Private Sequence to execute death
    private IEnumerator death() {
        invincible = true;
        animator.SetBool("Hurt", true);

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene("GameOver");
    }


    // Function to do I-frame sequence
    private IEnumerator invincibilityFrameSequence() {
        invincible = true;
        spriteRender.color = hurtColor;
        animator.SetBool("Hurt", true);

        yield return new WaitForSeconds(0.5f);

        animator.SetBool("Hurt", false);

        // Timer loop
        float timer = 0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
        bool inHurtColor = true;

        while (timer < invincibilityFrameDuration) {
            yield return waitFrame;

            timer += Time.fixedDeltaTime;
            spriteRender.color = (inHurtColor) ? Color.white : hurtColor;
            inHurtColor = !inHurtColor;
        }

        spriteRender.color = Color.white;
        invincible = false;
    }


    // Main function for handling movement: runs every frame
    private void FixedUpdate() {
        // Only do movement if you're moving and able to move (alive and not dashing)
        if (isMoving && curHealth > 0 && !dashing) {
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

        // Constantly check flip x
        if (isMoving || wallJumpSequence != null) {
            float animHorSpeed = (wallJumpSequence != null) ? rb.velocity.x : movementVector.x;
            spriteRender.flipX = (animHorSpeed < 0f);
        }

        // Constantly set animator
        animator.SetBool("WallSliding", wallSliding);
        animator.SetFloat("HorizontalSpeed", Mathf.Abs(movementVector.x));
        animator.SetFloat("VerticalSpeed", rb.velocity.y);
        animator.SetBool("Dashing", dashing);
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
        bool leftWallSlide;
        bool rightWallSlide;

        // If first time wall sliding, must move towards wall. If you previously wall slide, you must move the opposite direction
        if (prevWallSliding) {
            leftWallSlide = movementDir <= 0.0f && leftBlockerSensor.isBlocked();
            rightWallSlide = movementDir >= 0.0f && rightBlockerSensor.isBlocked();
        } else {
            leftWallSlide = movementDir < -0.0001 && leftBlockerSensor.isBlocked();
            rightWallSlide = movementDir > 0.0001 && rightBlockerSensor.isBlocked();

        }

        wallSliding = (leftWallSlide || rightWallSlide) && inAir && wallJumpSequence == null;

        // First frame of wall sliding
        if (wallSliding && !prevWallSliding && rb.velocity.y < 0f) {
            rb.velocity = Vector3.zero;
        }
    }


    // Event handler for jumping
    public void onJumpPress(InputAction.CallbackContext value) {
        if (value.started) {
            // Only do actual jump if not in pause menu
            if (!pauseMenu.menuActive()) {
                if (jumpsLeft > 0) {
                    // Apply jump
                    cancelDash();
                    playerAudio.playJumpSound();
                    rb.AddForce(initialJumpForce * Vector2.up);
                    jumpsLeft--;

                    // Set jumpPressed flag and check for tap jump
                    if (jumpCheckSequence != null) {
                        StopCoroutine(jumpCheckSequence);
                    }

                    jumpCheckSequence = StartCoroutine(checkJumpHeight());
                } else {
                    executeWallJump();
                }
            }

            jumpPressed = true;

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

        // If you are holding jump, extend jump
        if (jumpPressed) {
            rb.AddForce(extendedJumpForce * Vector2.up);
        }
    }


    // Main private helper function to check for wall jumps
    //  Pre: none
    //  Post: execute wall jump if you are moving towards a wall
    private void executeWallJump() {
        // You can only execute wall jump if you are in the air and falling down
        if (inAir) {

            // Check left wall
            if (leftBlockerSensor.isBlocked() && wallSliding) {
                Vector2 wallJumpVector = new Vector2(0.5f, 1f);
                playerAudio.playJumpSound();

                if (wallJumpSequence != null) {
                    StopCoroutine(wallJumpSequence);
                }
                wallJumpSequence = StartCoroutine(wallJumpExecution(wallJumpVector));
            }

            // Check right wall
            else if (rightBlockerSensor.isBlocked() && wallSliding) {
                Vector2 wallJumpVector = new Vector2(-0.5f, 1f);
                playerAudio.playJumpSound();

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


    // Main event handler for dashing
    public void onDashPress(InputAction.CallbackContext value) {
        if (value.started && !dashing && curHealth > 0 && canDash) {
            playerAudio.playDashSound();
            StartCoroutine(dashSequence());
        }
    }


    // Main dash IEnumerator
    //  Pre: player started dash by pressing a button
    //  Post: Player now in dashing sequence until maxDistance has passed OR dash gets canceled
    private IEnumerator dashSequence() {
        // Set dashing to true and timer up
        dashing = true;
        canDash = false;
        float distancePassed = 0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // Dash direction
        Vector2 dashDir = (spriteRender.flipX) ? Vector3.left : Vector3.right;
        IBlockerSensor checkedBlocker = (spriteRender.flipX) ? leftBlockerSensor : rightBlockerSensor;

        // Main dash loop
        while (distancePassed < maxDashDistance && dashing) {
            yield return waitFrame;

            // Get distance and translate
            float curDist = Time.fixedDeltaTime * dashSpeed;
            if (!checkedBlocker.isBlocked()) {
                transform.Translate(curDist * dashDir);
            }

            // Update timer
            distancePassed += curDist;
        }

        // Reverse dashing
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        dashing = false;

        // Cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }


    // Main function to cancel dash
    //  Pre: none
    //  Post: cancels dash if you're currently dashing
    private void cancelDash() {
        if (dashing) {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            dashing = false;
        }
    }


    // Event handler for when using the pause ability
    public void onAbilityPress(InputAction.CallbackContext value) {
        if (value.started && !pauseMenu.menuActive()) {
            if (pauseZone.canPause()) {
                cancelDash();

                // Pause at area 
                pauseZone.pause();
                playerAudio.playPauseSound();

                // Disable velocity, if you're in the air, add force
                rb.velocity = Vector2.zero;
                if (inAir) {
                    rb.velocity = Vector3.zero;
                    rb.AddForce(Vector3.up * pauseAirJumpForce);
                }
            }
        }
    }


    // Event handler for when pausing the game
    public void onPauseMenuPress(InputAction.CallbackContext value) {
        if (value.started) {
            if (pauseMenu.menuActive()) {
                pauseMenu.unpause();
            } else {
                pauseMenu.pause();
            }
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
        if (!enemyJumped) {
            rb.velocity = Vector3.zero;
            rb.AddForce(enemyJumpForce * Vector3.up);
            StartCoroutine(checkEnemyJumpSequence());
            playerAudio.playEnemyJumpSound();
        }
    }


    // Main wait sequence to check for enemy jump
    private IEnumerator checkEnemyJumpSequence() {
        enemyJumped = true;
        yield return new WaitForSeconds(0.1f);
        enemyJumped = false;
    }
}
