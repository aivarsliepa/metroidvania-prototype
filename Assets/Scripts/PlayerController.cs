using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rBody;
    public BoxCollider2D bCollider;
    public BoxCollider2D wallCheck;
    public BoxCollider2D groundCheck;


    [SerializeField] float moveSpeed = 600f;
    private float movement;
    private bool jump = false;

    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float wallJumpSideForce = 10f;

    [SerializeField] private int maxNumberOfJumps = 1;
    private int timesJumped = 0;
    private bool isFacingRight = true;

    private BoolUnityEvent groundEvent;
    private BoolUnityEvent wallEvent;

    private bool isAgainstWall = false;
    public float maxWallSlideVelocity = 1f;

    private bool isMovementEnabled = true;
    private float disabledMoveTimeAfterWallJump = 0.15f;

    // abilities
    public bool isWallJumpEnabled = true;
    public bool isDashEnabled = true;

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
            jump = true;
        }
    }

    void FixedUpdate()
    {
        FlipCheck();
        WallSlideCheck();
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
        if (jump)
        {
            if (maxNumberOfJumps > timesJumped)
            {
                timesJumped++;
                if (isAgainstWall)
                {
                    WallJump();
                } else
                {
                    Jump();
                }
            }
        }

        jump = false;
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
        StartCoroutine(WaitToEnableMovement());
    }

    IEnumerator WaitToEnableMovement()
    {
        isMovementEnabled = false;
        yield return new WaitForSeconds(disabledMoveTimeAfterWallJump);
        isMovementEnabled = true;
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
            ResetJumps();
        }
    }

    private void GroundTrigger(bool didEnter)
    {
        if (didEnter)
        {
            ResetJumps();
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

    private Vector2 GetBackDirection()
    {
        return isFacingRight ? Vector2.left : Vector2.right;
    }

    private void ResetJumps()
    {
        timesJumped = 0;
    }
}
