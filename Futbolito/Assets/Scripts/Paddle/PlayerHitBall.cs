using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitBall : MonoBehaviour {

    public float xVelOnPaddleHitFactor;
    public float levelOneForce, levelTwoForce, levelThreeForce;
    public int hitForce;

    

    private void OnCollisionEnter2D(Collision2D other)
    {
        detectCollistion(other.gameObject);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        detectCollistion(other.gameObject);
    }

    private void detectCollistion(GameObject obj)
    {
        if (obj.gameObject.tag == "Ball")
        {
            if (hitForce != 0)
            {
                //Get x Velocity
                float xPaddlePos = transform.position.x;
                float xBallPos = obj.transform.position.x;
                float xVel = (xBallPos - xPaddlePos) * xVelOnPaddleHitFactor;

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

                //Add force
                obj.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(xVel, yVel));
            }
        }
    }
}
