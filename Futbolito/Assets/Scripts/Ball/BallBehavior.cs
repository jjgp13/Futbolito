using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehavior : MonoBehaviour {

    public Rigidbody2D rb;
    public float initalBallForce;
    public GameObject ballExplosion;

    public float yVelDrag, wallHitDrag;
    public float xVelOnPaddleHit, xVelOnPaddleHold;
    

	// Use this for initialization
	void Start () {
        Invoke("AddInitialVelocity", 5f);
	}

    void AddInitialVelocity()
    {
        Vector2 initialVel = new Vector2(Random.Range(-initalBallForce, initalBallForce), Random.Range(-initalBallForce, initalBallForce));
        rb.AddForce(initialVel);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        detectCollision(other.gameObject);
    }

    private void detectCollision(GameObject obj)
    {
        if (obj.gameObject.tag == "PlayerPaddle") PlayerHitBall(obj);
        if (obj.gameObject.tag == "NPCPaddle") NPCHitBall(obj);
        if (obj.gameObject.tag == "Wall") BallHitAgainstWall(obj);
    }

    void PlayerHitBall(GameObject obj)
    {
        float yForce = obj.GetComponent<PlayerAnimationController>().yForce;
        float xForce = obj.GetComponent<PlayerAnimationController>().xForce;
        float xPaddlePos = obj.transform.position.x;
        float xBallPos = transform.position.x;
        float xVel = Mathf.Abs(xBallPos - xPaddlePos) * xForce;
        if (xBallPos < xPaddlePos) xVel = -xVel;

        if (yForce != 0)
        {
            //Add force
            rb.AddForce(new Vector2(xVel, yForce));
            rb.AddTorque(xVel, ForceMode2D.Impulse);
        }
        else
        {
            Vector2 vel = rb.velocity;
            vel.y = vel.y / yVelDrag;
            vel.x = xVel;
            rb.velocity = vel;
        }
    }

    void NPCHitBall(GameObject obj)
    {
        float shootSpeed = obj.GetComponent<NPCStats>().shootSpeed;
        float xPaddlePos = obj.transform.position.x;
        float xBallPos = transform.position.x;
        float xVel;

        if (obj.GetComponent<NPCStats>().isShooting)
        {
            
            xVel = Mathf.Abs(xBallPos - xPaddlePos) * shootSpeed;
            if (xBallPos < xPaddlePos) xVel = -xVel;

            float yVel = -shootSpeed;

            //Add force
            rb.AddForce(new Vector2(xVel, yVel));
            rb.AddTorque(xVel,ForceMode2D.Impulse);
        }
        else
        {
            Vector2 vel = rb.velocity;
            vel.y = vel.y / yVelDrag;

            xVel = Mathf.Abs(xBallPos - xPaddlePos) * xVelOnPaddleHold;
            vel.x = xVel;
            rb.velocity = vel;
        }
    }

    public void BallHitAgainstWall(GameObject obj)
    {
        Vector2 vel = rb.velocity;
        vel *= wallHitDrag;
        rb.velocity = vel;
    }

    private void OnDestroy()
    {
        Instantiate(ballExplosion, gameObject.transform.position, Quaternion.identity);
    }
}