using UnityEngine;
using UnityEngine.UI;

public class PlayerAnimationController : MonoBehaviour {

    public LinesHandler linesHandler;
    private string shootButton;
    private string attractButton;

    public Animator animatorController;
    public float shootForce;
    public float attractionForce;

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
                direction.Normalize();
                direction *= shootForce;
                //Debug.Log("Point of contact: " + pointOfContact + "Velocity: " + direction);
                objectHitted.GetComponent<Rigidbody2D>().AddForceAtPosition(direction, pointOfContact, ForceMode2D.Impulse);
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
