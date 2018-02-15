using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehavior : MonoBehaviour {

    public Rigidbody2D rb;
    public float xVelOnPaddleHit;

	// Use this for initialization
	void Start () {
        /*Vector2 initialVel = new Vector2(Random.Range(-100, 100), Random.Range(-100, 100));
        rb.AddForce(initialVel);*/
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        //print(rb.velocity);
	}

    private void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.tag == "Paddle")
        {
            if (other.contacts[0].normal.y > 0 && other.gameObject.GetComponent<PlayerAnimationController>().hitForce != 0)
            {
                //Get x Velocity
                float xBallPos = transform.position.x;
                float xPaddlePos = other.transform.position.x;
                float xVel = (xBallPos - xPaddlePos) * xVelOnPaddleHit;


                //Get Y velocity
                int hitPaddleForce = other.gameObject.GetComponent<PlayerAnimationController>().hitForce;
                float yVel = 0;
                switch (hitPaddleForce)
                {
                    case 1:
                        yVel = 30f;
                        break;
                    case 2:
                        yVel = 60f;
                        break;
                    case 3:
                        yVel = 100f;
                        break;
                    default:
                        break;
                }

                //Add force
                rb.AddForce(new Vector2(xVel, yVel));
            }
        }
    }
}
