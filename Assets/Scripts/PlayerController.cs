using UnityEditor.UI;
using UnityEngine;

public class PlayerController : ObjectWithHealth
{
    // components
    public Rigidbody2D rBody;
    public BoxCollider2D bCollider;
    public BoxCollider2D wallCheck;
    public BoxCollider2D groundCheck;
    public Animator animator;

    // prefabs
    [SerializeField] private GameObject bulletPrefab = default;

    // configuration
    [SerializeField] private float moveSpeed = 600f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float wallJumpSideForce = 10f;
    [SerializeField] private float maxWallSlideVelocity = 2f;
    [SerializeField] private float maxGlideVelocity = 1f;
    [SerializeField] private float disabledMoveTimeAfterWallJump = 0.15f;
    [SerializeField] private float dashSpeed = 2000f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float groundSlamSpeed = 2000f;

    // stats
    [SerializeField] private int maxNumberOfDashes = 1;
    [SerializeField] private int maxNumberOfJumps = 1;
    [SerializeField] private int maxEnergy = 2;


    // events
    private BoolUnityEvent groundEvent;
    private BoolUnityEvent wallEvent;

    // state
    private int timesJumped = 0;
    private int timesDashed = 0;
    private int currentEnergy;
    private bool isFacingRight = true;
    private bool isAgainstWall = false;
    private bool isMovementEnabled = true;
    private bool isDashing = false;
    private bool isGrounded = true;

    // abilities
    public bool isWallJumpEnabled = true;
    public bool isDashEnabled = true;
    public bool isGroundSlamEnabled = true;
    public bool isGlideEnabled = true;
    public bool isShootingEnabled = true;

    // player actions
    private float horizontalMovement;
    private bool jumpAction = false;
    private bool dashAction = false;
    private bool groundSlamAction = false;
    private bool glideAction = false;
    private bool shootAction = false;


    public float bulletForce = 30f;

    private new void Awake()
    {
        base.Awake();
        groundEvent = new BoolUnityEvent();
        groundCheck.GetComponent<TriggerCheck>().triggerEvent = groundEvent;
        groundEvent.AddListener(GroundTrigger);

        wallEvent = new BoolUnityEvent();
        wallCheck.GetComponent<TriggerCheck>().triggerEvent = wallEvent;
        wallEvent.AddListener(WallTrigger);

        currentEnergy = maxEnergy;
    }

    private void Update()
    {
        horizontalMovement = Input.GetAxis(Constants.Input.HORIZONTAL);
        var verticalMovement = Input.GetAxisRaw(Constants.Input.VERTICAL);

        animator.SetBool(Constants.PlayerAnimator.RUNNING, horizontalMovement != 0);

        if (isGroundSlamEnabled && !isGrounded && verticalMovement == -1)
        {
            groundSlamAction = true;
            return;
        }

        if (Input.GetButtonDown(Constants.Input.JUMP))
        {
            jumpAction = true;
        }

        if (Input.GetButtonDown(Constants.Input.DASH))
        {
            if (isDashEnabled)
            {
                dashAction = true;
            }
        }

        if (Input.GetButtonDown(Constants.Input.FIRE))
        {
            if (isShootingEnabled)
            {
                shootAction = true;
            }
        }


        if (isGlideEnabled)
        {
            var glideAxis = Input.GetAxisRaw(Constants.Input.GLIDE);
            if (!isGrounded && (glideAxis == 1 || glideAxis == -1))
            {
                glideAction = true;
            }
            else
            {
                glideAction = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (groundSlamAction)
        {
            GroundSlam();
            return;
        }

        if (dashAction || isDashing)
        {
            ApplyDash();
            return;
        }

        ApplyShooting();
        ApplyGliding();
        WallSlideCheck();
        ApplyJumping();
        ApplyHorizontalMovement();
    }

    private void ApplyHorizontalMovement()
    {
        if (!isMovementEnabled) return;

        FlipCheck(); // might want to remove this ?? 
        Vector3 targetVelocity = new Vector2(moveSpeed * horizontalMovement * Time.fixedDeltaTime, rBody.velocity.y);
        rBody.velocity = targetVelocity;
    }

    private void ApplyJumping()
    {
        if (jumpAction && CanJump())
        {
            timesJumped++;
            if (isAgainstWall)
            {
                WallJump();
            }
            else
            {
                Jump();
            }
        }

        jumpAction = false;
    }

    private void Jump()
    {
        ResetHorizontalVelocity();
        rBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }



    private void WallJump()
    {
        Flip();
        ResetHorizontalVelocity();
        var jumpVector = new Vector2(GetDirectionVector().x * wallJumpSideForce, 1 * jumpForce);
        rBody.AddForce(jumpVector, ForceMode2D.Impulse);

        isMovementEnabled = false;
        StartCoroutine(Utils.WaitForAction(disabledMoveTimeAfterWallJump, () => isMovementEnabled = true));
    }

    private void ResetHorizontalVelocity()
    {
        rBody.velocity = new Vector2(rBody.velocity.x, 0f);
    }

    private void FlipCheck()
    {
        if (horizontalMovement < 0 && isFacingRight)
        {
            Flip();
        }
        else if (horizontalMovement > 0 && !isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void WallTrigger(bool didEnter)
    {
        isAgainstWall = didEnter;
        if (didEnter && isWallJumpEnabled)
        {
            ResetActions();
        }
    }

    private void GroundTrigger(bool didEnter)
    {
        isGrounded = didEnter;
        if (didEnter)
        {
            ResetActions();
            groundSlamAction = false;
        }
    }

    private void WallSlideCheck()
    {
        if (isWallJumpEnabled && isAgainstWall)
        {
            if ((isFacingRight && horizontalMovement > 0) || (!isFacingRight && horizontalMovement < 0))
            {
                Vector3 targetVelocity = new Vector2(rBody.velocity.x, Mathf.Max(rBody.velocity.y, -maxWallSlideVelocity));
                rBody.velocity = targetVelocity;
            }
        }
    }

    private void ApplyGliding()
    {
        if (!isAgainstWall && glideAction)
        {
            Vector3 targetVelocity = new Vector2(rBody.velocity.x, Mathf.Max(rBody.velocity.y, -maxGlideVelocity));
            rBody.velocity = targetVelocity;
        }
    }

    private void ApplyShooting()
    {
        if (shootAction && currentEnergy > 0)
        {
            var bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<Rigidbody2D>().AddForce(GetDirectionVector() * bulletForce, ForceMode2D.Impulse);

            currentEnergy--;
        }

        shootAction = false;
    }

    private Vector2 GetDirectionVector()
    {
        return isFacingRight ? Vector2.right : Vector2.left;
    }

    private void ResetJumps()
    {
        timesJumped = 0;
    }

    private void ResetDash()
    {
        timesDashed = 0;
    }

    private void ResetActions()
    {
        ResetDash();
        ResetJumps();
    }

    private bool CanJump()
    {
        return isGrounded || maxNumberOfJumps > timesJumped;
    }

    private bool CanDash()
    {
        return maxNumberOfDashes > timesDashed;
    }

    private void ApplyDash()
    {
        if (!isDashing && dashAction && CanDash())
        {
            isDashing = true;
            isMovementEnabled = false;
            timesDashed++;
            StartCoroutine(Utils.WaitForAction(dashTime, () =>
            {
                isDashing = false;
                isMovementEnabled = true;
                if (isGrounded)
                {
                    ResetDash();
                }
            }));
        }

        if (isDashing)
        {
            Dash();
        }

        dashAction = false;
    }

    private void Dash()
    {
        rBody.velocity = new Vector2(dashSpeed * GetDirectionVector().x * Time.fixedDeltaTime, 0);
    }

    private void GroundSlam()
    {
        isDashing = false;
        rBody.velocity = new Vector2(0, groundSlamSpeed * -1 * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(Constants.Tags.ENEMY))
        {
            DamageBy(1);
        }
    }
}
