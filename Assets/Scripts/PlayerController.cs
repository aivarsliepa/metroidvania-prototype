using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rBody;
    public BoxCollider2D bCollider;
    public BoxCollider2D wallCheck;
    public CircleCollider2D groundCheck;


    [SerializeField] float moveSpeed = 600f;
    private float movement;
    private bool jump = false;
    private bool isGrounded = false;

    [SerializeField] private float jumpForce = 3f;
    [SerializeField] private float wallJumpSideForce = 1f;

    [SerializeField] private int maxJumps = 1;
    public int timesJumped = 0;
    private bool isFacingRight = true;

    private BoolUnityEvent groundEvent;
    private BoolUnityEvent wallEvent;

    public bool isAgainstWall = false;
    public float maxWallSlideVelocity = 1f;

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
        ApplyHorizontalMovement();
        ApplyJumping();
    }

    private void ApplyHorizontalMovement()
    {
        Vector3 targetVelocity = new Vector2(moveSpeed * movement * Time.fixedDeltaTime, rBody.velocity.y);
        rBody.velocity = targetVelocity;
    }

    private void ApplyJumping()
    {
        if (jump)
        {
            if (maxJumps > timesJumped)
            {
                Jump();
            }
            else
            {
                if (isGrounded)
                {
                    Jump();
                }
            }
        }

        jump = false;
    }

    private void Jump()
    {
        rBody.velocity = new Vector2(rBody.velocity.x, 0f); // reset velocity when falling down
        rBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        timesJumped++;
        isGrounded = false;
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
    }

    private void GroundTrigger(bool didEnter)
    {
        if (didEnter)
        {
            isGrounded = true;
            timesJumped = 0;
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
}
