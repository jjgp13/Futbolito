using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoosballFigureWallPassAction : MonoBehaviour
{
    [SerializeField] public float wallPassForce = 30f;
    private bool canPerformWallPass = false;
    private Rigidbody2D ballRb;
    private GameObject ball;

    private void Start()
    {
        // Ensure the AreaEffector has trigger collider
        CircleCollider2D collider = GetComponentInChildren<CircleCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            canPerformWallPass = true;
            ball = collision.gameObject;
            ballRb = collision.GetComponent<Rigidbody2D>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            canPerformWallPass = false;
            ball = null;
            ballRb = null;
        }
    }

    public bool CanPerformWallPass()
    {
        return canPerformWallPass && ball != null && ballRb != null;
    }

    public void PerformWallPass()
    {
        if (CanPerformWallPass())
        {
            // Stop the ball's velocity
            ballRb.linearVelocity = Vector2.zero;
            
            // Determine direction based on ball position relative to parent
            Vector2 direction;
            if (ball.transform.position.y > transform.position.y)
            {
                // Ball is above the figure, apply upward force
                direction = Vector2.up;
            }
            else
            {
                // Ball is below the figure, apply downward force
                direction = Vector2.down;
            }
            
            // Apply impulse in the determined direction
            ballRb.AddForce(direction * wallPassForce, ForceMode2D.Impulse);
            
            // TODO: either an animation or some shader
            // Trigger animation if needed
            //FoosballFigureAnimationController controller = GetComponent<FoosballFigureAnimationController>();
            //if (controller != null)
            //{
            //    controller.TriggerWallPassAnimation();
            //}
        }
    }
}
