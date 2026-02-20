using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHitBall : MonoBehaviour
{
    public bool isShooting;
    public float shootForce;
    private readonly float[] shootForces = new float[3] { 1.5f, 2f, 2.5f };

    [Header("Reference to attracting particles")]
    public ParticleSystem attractingParticles;

    private int npcDifficulty;

    // Start is called before the first frame update
    void Start()
    {
        isShooting = false;

        if (MatchInfo.instance != null)
        {
            npcDifficulty = MatchInfo.instance.matchLevel - 1;
        }
        else
        {
            npcDifficulty = 2;
        }
        
        shootForce = shootForces[npcDifficulty];
    }

    //Check if the object hitted is the ball
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleBallCollision(collision);
    }

    // Catches balls already in contact when shooting begins
    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleBallCollision(collision);
    }

    private void HandleBallCollision(Collision2D collision)
    {
        GameObject objectHitted = collision.gameObject;
        ContactPoint2D firstContact = collision.GetContact(0);
        if (objectHitted.tag == "Ball")
        {

            //if is playing shoot animation
            if (isShooting)
            {
                //Get point of contact
                Vector2 pointOfContact = firstContact.point;
                //Get direction vector
                Vector2 direction = objectHitted.transform.position - gameObject.transform.position;

                //Try decrementing y velocity
                if (direction.x < direction.y) direction.y /= 2f;

                //Normalized 
                direction.Normalize();
                //Multiplay by shoot force
                direction *= shootForce;
                
                //Add force to ball
                objectHitted.GetComponent<Rigidbody2D>().AddForceAtPosition(direction, pointOfContact, ForceMode2D.Impulse);

                isShooting = false;
            }
        }
    }

}
