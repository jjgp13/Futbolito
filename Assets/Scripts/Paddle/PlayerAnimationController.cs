using UnityEngine;
using UnityEngine.UI;

public class PlayerAnimationController : MonoBehaviour {

    public LinesHandler linesHandler;
    private string shootButton;
    private string attractButton;
    private string wallPassButton;

    public Animator animatorController;
    public float shootForce;
    public float attractionForce;
    public float wallPassForce;

    float shootHoldingTime = 0;
    float attractHoldingTime = 0;

    public ParticleSystem attractBall;
    public ParticleSystem chargingShoot;

    private void Start()
    {
        linesHandler = transform.parent.GetComponentInParent<LinesHandler>();
        if(linesHandler.numberOfPlayers == 1)
        {
            shootButton = linesHandler.defenseButtons.shootButton;
            attractButton = linesHandler.defenseButtons.attractButton;
            wallPassButton = linesHandler.defenseButtons.wallPassButton;
        }
        else
        {
            //Controls for two players
            //Think
        }
        attractBall.Stop();
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (IsParentLineActive())
        {
            CheckShooting();
            CheckMagnet();
        }
        else
        {
            shootHoldingTime = 0;
            attractHoldingTime = 0;
            if (attractBall.isPlaying) attractBall.Stop();
            if (chargingShoot.isPlaying) chargingShoot.Stop();
            animatorController.SetBool("touching", false);
            animatorController.SetFloat("timeTouching", 0);
        }
    }

    //Check if the object hitted is the ball
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject objectHitted = collision.gameObject;
        ContactPoint2D firstContact = collision.GetContact(0);
        if(objectHitted.tag == "Ball")
        {
            //if is playing shoot animation
            if(shootForce > 0)
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
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        GameObject objectHitted = collision.gameObject;
        if (GetComponentInParent<LineMovement>().isActive)
        {
            //If ball is inside of trigger and press wall pass
            if (objectHitted.tag == "Ball" && Input.GetButtonDown(wallPassButton))
            {
                //If ball is in movement, stops it
                if(collision.gameObject.GetComponent<Rigidbody2D>().velocity.x > 0 && collision.gameObject.GetComponent<Rigidbody2D>().velocity.y > 0)
                    collision.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0f,0f);
                else//If ball is quite, make wall pass
                {
                    int randomWall = Random.Range(0, 100);
                    //pass to upper wall
                    if (randomWall >= 50) collision.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, wallPassForce);
                    else collision.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, -wallPassForce);
                }
            }
        }
    }

    private bool IsParentLineActive()
    {
        return gameObject.GetComponentInParent<LineMovement>().isActive;
    }
    
    private void CheckShooting()
    {
        if (Input.GetButton(shootButton))
        {
            shootHoldingTime += Time.deltaTime;
            if (!chargingShoot.isPlaying) chargingShoot.Play();
            var emission = chargingShoot.emission;
            emission.rateOverTime = shootHoldingTime * 50f;

            animatorController.SetBool("touching", true);
            animatorController.SetFloat("timeTouching", shootHoldingTime);
        }
        else
        {
            shootHoldingTime = 0;
            if (chargingShoot.isPlaying)
            {
                chargingShoot.Stop();
                var emission = chargingShoot.emission;
                emission.rateOverTime = 0;
            }

            animatorController.SetBool("touching", false);
            animatorController.SetFloat("timeTouching", shootHoldingTime);
        }
    }

    private void CheckMagnet()
    {
        if (Input.GetButton(attractButton))
        {
            attractHoldingTime += Time.deltaTime;
            if (attractHoldingTime > 0)
            {
                GetComponent<PointEffector2D>().forceMagnitude = attractionForce;
                if (!attractBall.isPlaying)
                    attractBall.Play();
            }
        }
        else
        {
            attractHoldingTime = 0;
            GetComponent<PointEffector2D>().forceMagnitude = 0;
            attractBall.Stop();
        }
    }

}
