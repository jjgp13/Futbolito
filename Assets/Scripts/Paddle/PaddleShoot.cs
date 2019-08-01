using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleShoot : MonoBehaviour
{
    /// <summary>
    /// References to info that is on paddle controller
    /// </summary>
    private string shootButton;
    private Animator paddleAnimator;

    [Header("Charging shoot particles reference")]
    public ParticleSystem chargingShoot;

    /// <summary>
    /// Variables for shoot
    /// </summary>
    [Header("Shoot forces applied when paddle is shooting")]
    public bool shootLevelOne;
    [Range(2, 3)] public float shootForceOne;

    public bool shootLevelTwo;
    [Range(3, 4)] public float shootForceTwo;
    
    public bool shootLevelThree;
    [Range(4, 5)]  public float shootForceThree;


    private float shootHoldingTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        shootButton = GetComponent<PaddleController>().shootButton;
        paddleAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (GetComponent<PaddleController>().isLineActive)
        {
            if (Input.GetButton(shootButton))
                ChargingShoot();

            if (Input.GetButtonUp(shootButton))
                Shoot();
        }
    }

    private void ChargingShoot()
    {
        shootHoldingTime += Time.deltaTime;
        if (!chargingShoot.isPlaying) chargingShoot.Play();
        var emission = chargingShoot.emission;
        emission.rateOverTime = shootHoldingTime * 50f;
        //Pass info to animator
        paddleAnimator.SetBool("touching", true);
        paddleAnimator.SetFloat("timeTouching", shootHoldingTime);
    }

    private void Shoot()
    {
        shootHoldingTime = 0;
        if (chargingShoot.isPlaying)
        {
            chargingShoot.Stop();
            var emission = chargingShoot.emission;
            emission.rateOverTime = 0;
        }
        //Pass infor to animator
        paddleAnimator.SetBool("touching", false);
        paddleAnimator.SetFloat("timeTouching", shootHoldingTime);
    }

    //Check if the object hitted is the ball
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject objectHitted = collision.gameObject;
        ContactPoint2D firstContact = collision.GetContact(0);
        if (objectHitted.tag == "Ball")
        {
            float shootForce = ReturnLevelHitForce();
            Debug.Log(shootForce);
            //if is playing shoot animation
            if (shootForce > 0)
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
                //BulletTimeController.instance.DoSlowMotion();
            }
        }
    }

    private float ReturnLevelHitForce()
    {
        if (shootLevelOne)
            return shootForceOne;
        if (shootLevelTwo)
            return shootForceTwo;
        if (shootLevelThree)
            return shootForceThree;
        return 0;
    }
}
