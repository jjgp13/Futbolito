using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehavior : MonoBehaviour {

    Rigidbody2D rb;
<<<<<<< HEAD
    public int initalBallForce;
=======
>>>>>>> fb1a5c5093cc19aa6a88c9996de18e658ea1180b
    public GameObject ballExplosion;

    [Range(0, 50)]
    public float initalBallForce;
    [Header("Values between 0 and 1")]
    public Vector2 DecreaseFactor;

    [Range(0f, 1f)]
    public float wallHitDrag;
    
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
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
            Vector2 newVel = rb.velocity;
            newVel.x *= DecreaseFactor.x;
            newVel.y *= DecreaseFactor.y;
            rb.velocity = newVel;
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
            Vector2 newVel = rb.velocity;
            newVel.x *= DecreaseFactor.x;
            newVel.y *= DecreaseFactor.y;
            rb.velocity = newVel;
        }
    }

    public void BallHitAgainstWall(GameObject obj)
    {
        Vector2 vel = rb.velocity;
        vel *= wallHitDrag;
        rb.velocity = vel;
    }
}