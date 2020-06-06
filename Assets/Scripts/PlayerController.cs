using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // components
    public Rigidbody2D rBody;
    public BoxCollider2D bCollider;
    public BoxCollider2D wallCheck;
    public BoxCollider2D groundCheck;

    // configuration
    [SerializeField] private float moveSpeed = 600f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float wallJumpSideForce = 10f;
    [SerializeField] private int maxNumberOfJumps = 1;
    [SerializeField] private int maxNumberOfDashes = 1;
    [SerializeField] private float maxWallSlideVelocity = 1f;
    [SerializeField] private float disabledMoveTimeAfterWallJump = 0.15f;
    [SerializeField] private float dashSpeed = 2000f;
    [SerializeField] private float dashTime = 0.2f;

    // events
    private BoolUnityEvent groundEvent;
    private BoolUnityEvent wallEvent;

    // state
    private int timesJumped = 0;
    private int timesDashed = 0;
    private bool isFacingRight = true;
    private bool isAgainstWall = false;
    private bool isMovementEnabled = true;
    private bool isDashing = false;
    private bool isGrounded = true;

    // abilities
    public bool isWallJumpEnabled = true;
    public bool isDashEnabled = true;

    // player actions
    private float movement;
    private bool jumpAction = false;
    private bool dashAction = false;

    private void Awake()
    {
        groundEvent = new BoolUnityEvent();
        groundCheck.GetComponent<TriggerCheck>().triggerEvent = groundEvent;
        groundEvent.AddListener(GroundTrigger);

        wallEvent = new BoolUnityEvent();
        wallCheck.GetComponent<TriggerCheck>().triggerEvent = wallEvent;
        wallEvent.AddListener(WallTrigger);
    }

    private void Update()
    {
        movement = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            jumpAction = true;
        }

        if (Input.GetButtonDown("Dash"))
        {
            if (isDashEnabled)
            {
                dashAction = true;
            }
        }
    }

    void FixedUpdate()
    {
        FlipCheck();
        WallSlideCheck();
        ApplyDash();
        ApplyJumping();
        ApplyHorizontalMovement();
    }

    private void ApplyHorizontalMovement()
    {
        if (!isMovementEnabled) return;

        Vector3 targetVelocity = new Vector2(moveSpeed * movement * Time.fixedDeltaTime, rBody.velocity.y);
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
        ResetHorizontalVelocity();
        var jumpVector = new Vector2(GetBackDirection().x * wallJumpSideForce, 1 * jumpForce);
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
        if (movement < 0 && isFacingRight)
        {
            Flip();
        }
        else if (movement > 0 && !isFacingRight)
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
        }
    }

    private void WallSlideCheck()
    {
        if (isAgainstWall)
        {
            if ((isFacingRight && movement > 0) || (!isFacingRight && movement < 0))
            {
                Vector3 targetVelocity = new Vector2(rBody.velocity.x, Mathf.Max(rBody.velocity.y, -maxWallSlideVelocity));
                rBody.velocity = targetVelocity;
            }
        }
    }

    private Vector2 GetDirectionVector()
    {
        return isFacingRight ? Vector2.right : Vector2.left;
    }

    private Vector2 GetBackDirection()
    {
        return GetDirectionVector() * -1;
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
        return maxNumberOfJumps > timesJumped;
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
}
