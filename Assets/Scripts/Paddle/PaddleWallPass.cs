using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleWallPass : MonoBehaviour
{

    public string wallPassButton;
    public float wallPassForce;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    private void OnTriggerStay2D(Collider2D collision)
    {
        GameObject objectHitted = collision.gameObject;
        if (GetComponentInParent<LineMovement>().isActive)
        {
            //If ball is inside of trigger and press wall pass
            if (objectHitted.tag == "Ball" && Input.GetButtonDown(wallPassButton))
            {
                if (!BulletTimeController.instance.inSlowMotion)
                    BulletTimeController.instance.DoSlowMotion(0.25f, 1f);

                collision.gameObject.GetComponent<Rigidbody2D>().velocity *= 0.1f;
                collision.gameObject.GetComponent<BallBehavior>().stopBallParticles.Play();
                //If ball is in movement, stops it
                //if(collision.gameObject.GetComponent<Rigidbody2D>().velocity.x > 0 && collision.gameObject.GetComponent<Rigidbody2D>().velocity.y > 0)
                //    
                //else//If ball is quite, make wall pass
                //{
                //    int randomWall = Random.Range(0, 100);
                //    //pass to upper wall
                //    if (randomWall >= 50) collision.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, wallPassForce);
                //    else collision.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, -wallPassForce);
                //}
            }
        }
    }
}
