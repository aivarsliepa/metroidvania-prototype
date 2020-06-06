using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rBody;
    public BoxCollider2D bCollider;
    [SerializeField] float moveSpeed = 600f;
    float movement;
    bool jump = false;
    private bool isGrounded = false;

    [SerializeField] private Transform groundCheck = default;
    [SerializeField] private float jumpForce = 3f;
    [SerializeField] private LayerMask whatIsGround = default;
    [SerializeField] private float groundedRadius = .4f;

    // skills
    [SerializeField] private int maxJumps = 1;
    public int timesJumped = 0;

    private void Update()
    {
        movement = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            Debug.Log("Jump");
            jump = true;
        }
    }

    void FixedUpdate()
    {
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
                GroundCheck();
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
        rBody.velocity = new Vector2(rBody.velocity.x, 0f);
        rBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        timesJumped++;
        isGrounded = false;
    }

    void GroundCheck()
    {
        if (isGrounded) return;
        Debug.Log("Ground check");
        var colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, whatIsGround);
        if (colliders.Length > 0)
        {
            isGrounded = true;
            timesJumped = 0;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(groundCheck.position, groundedRadius);
    }
}
