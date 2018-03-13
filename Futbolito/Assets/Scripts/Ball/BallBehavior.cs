using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehavior : MonoBehaviour {

    public Rigidbody2D rb;
    public float initalBallForce;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        Invoke("AddInitialVelocity", 3f);
	}

    private void Update()
    {
        //print(rb.velocity);
        //if (rb.velocity == Vector2.zero) AddVelocity();
        //print(rb.velocity);
    }

    void AddInitialVelocity()
    {
        Vector2 initialVel = new Vector2(Random.Range(-initalBallForce, initalBallForce), Random.Range(-initalBallForce, initalBallForce));
        rb.AddForce(initialVel);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Vector2 normal = other.contacts[0].normal;
        detectCollision(other.gameObject, normal);
    }

    private void detectCollision(GameObject obj, Vector2 normal)
    {
        if (obj.gameObject.tag == "PlayerPaddle") PlayerHitBall(obj, normal);
        if (obj.gameObject.tag == "NPCPaddle") NPCHitBall(obj, normal);
        if (obj.gameObject.tag == "Wall") BallHitAgainstWall(obj, normal);
    }

    void PlayerHitBall(GameObject obj, Vector2 normal)
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
            vel.y = vel.y / 3;
            vel.x = xVel;
            rb.velocity = vel;
        }
    }

    void NPCHitBall(GameObject obj, Vector2 normal)
    {
        float shootSpeed = obj.GetComponent<NPCStats>().shootSpeed;
        float xPaddlePos = obj.transform.position.x;
        float xBallPos = transform.position.x;
        float xVel;

        if (obj.GetComponent<NPCStats>().isShooting)
        {
            
            xVel = Mathf.Abs(xBallPos - xPaddlePos) * 75;
            if (xBallPos < xPaddlePos) xVel = -xVel;

            float yVel = -shootSpeed;

            //Add force
            rb.AddForce(new Vector2(xVel, yVel));
            rb.AddTorque(xVel,ForceMode2D.Impulse);
        }
        else
        {
            Vector2 vel = rb.velocity;
            vel.y = vel.y / 3;

            xVel = Mathf.Abs(xBallPos - xPaddlePos) * 10;
            vel.x = xVel;
            rb.velocity = vel;
        }
    }

    public void BallHitAgainstWall(GameObject obj, Vector2 normal)
    {
        Vector2 vel = rb.velocity;
        vel *= 0.8f;
        rb.velocity = vel;
    }
}