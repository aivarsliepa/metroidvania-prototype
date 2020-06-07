using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // config
    [SerializeField] private float moveSpeed = 150f;

    // components
    public Rigidbody2D rBody;

    private Vector2 moveDirection;

    private void Awake()
    {
        moveDirection = Vector2.left;
    }

    private void FixedUpdate()
    {
        Vector3 targetVelocity = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rBody.velocity = targetVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(Constants.Tags.PLATFORM))
        {
            moveDirection *= -1;
        }
    }
}
