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
        if (gameObject.GetComponentInParent<LineMovement>().isActive)
        {

            if (Input.GetButton(shootButton))
            {
                shootHoldingTime += Time.deltaTime;
                if(!chargingShoot.isPlaying) chargingShoot.Play();
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

            if (Input.GetButton(attractButton))
            {
                attractHoldingTime += Time.deltaTime;
                if(attractHoldingTime > 0)
                {
                    GetComponent<PointEffector2D>().forceMagnitude = attractionForce;
                    if(!attractBall.isPlaying)
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
    

}
