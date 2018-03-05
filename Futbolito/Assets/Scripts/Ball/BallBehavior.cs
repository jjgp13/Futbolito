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

    private void OnCollisionStay2D(Collision2D other)
    {
        Vector2 normal = other.contacts[0].normal;
        detectCollision(other.gameObject, normal);
    }

    private void detectCollision(GameObject obj, Vector2 normal)
    {
        if (obj.gameObject.tag == "PlayerPaddle") PlayerHitBall(obj, normal);
        if (obj.gameObject.tag == "NPCPaddle") NPCHitBall(obj, normal);
    }

    void PlayerHitBall(GameObject obj, Vector2 normal)
    {
        int hitForce = obj.GetComponent<PlayerAnimationController>().hitForce;
        float xVelOnPaddleHitFactor = obj.GetComponent<PlayerAnimationController>().xVelOnPaddleHitFactor;
        float levelOneForce = obj.GetComponent<PlayerAnimationController>().levelOneForce;
        float levelTwoForce = obj.GetComponent<PlayerAnimationController>().levelTwoForce;
        float levelThreeForce = obj.GetComponent<PlayerAnimationController>().levelThreeForce;

        if (hitForce != 0)
        {
            //Get x Velocity
            float xPaddlePos = obj.transform.position.x;
            float xBallPos = transform.position.x;
            float xVel = Mathf.Abs(xBallPos - xPaddlePos) * xVelOnPaddleHitFactor;
            if (xBallPos < xPaddlePos) xVel = -xVel;

            //Get Y velocity
            float yVel = 0;
            switch (hitForce)
            {
                case 1:
                    yVel = levelOneForce;
                    break;
                case 2:
                    yVel = levelTwoForce;
                    break;
                case 3:
                    yVel = levelThreeForce;
                    break;
                default:
                    break;
            }
            //yVel = yVel * normal.y;

            //Add force
            rb.AddForce(new Vector2(xVel, yVel));
            rb.AddTorque(xVel);
        }
        else
        {
            Vector2 vel = rb.velocity;
            vel /= 2;
            rb.velocity = vel;
        }
    }

    void NPCHitBall(GameObject obj, Vector2 normal)
    {
        if (obj.GetComponent<NPCStats>().isShooting)
        {
            float shootSpeed = obj.GetComponent<NPCStats>().shootSpeed;
            float xPaddlePos = obj.transform.position.x;
            float xBallPos = transform.position.x;
            float xVel = Mathf.Abs(xBallPos - xPaddlePos) * 25;
            if (xBallPos < xPaddlePos) xVel = -xVel;

            //float yVel = shootSpeed * normal.y;
            float yVel = -shootSpeed;

            //Add force
            rb.AddForce(new Vector2(xVel, yVel));
            rb.AddTorque(xVel);
        }
    }
}
