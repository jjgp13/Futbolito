using UnityEngine;
using UnityEngine.UI;

public class PlayerAnimationController : MonoBehaviour {

    public Animator animatorController;

    public float xForce, yForce;

    public ParticleSystem attractBall;

    private void Start()
    {
        attractBall.Stop();
    }

    // Update is called once per frame
    void Update () {
        if (gameObject.GetComponentInParent<LineMovement>().isActive)
        {
            if (ShootButton._shootButton.isShooting)
            {
                animatorController.SetBool("touching", true);
                animatorController.SetFloat("timeTouching", ShootButton._shootButton.holdingTime);
            }
            else
            {
                animatorController.SetBool("touching", false);
                animatorController.SetFloat("timeTouching", ShootButton._shootButton.holdingTime);
            }

            if (HoldButton._holdButton.isHolding)
            {
                if(HoldButton._holdButton.availableTime > 0 && !HoldButton._holdButton.empty)
                {
                    GetComponent<PointEffector2D>().forceMagnitude = -1;
                    if(!attractBall.isPlaying)
                        attractBall.Play();
                }
                    
            }
            else
            {
                GetComponent<PointEffector2D>().forceMagnitude = 0;
                attractBall.Stop();
            }
        } else
            if(attractBall.isPlaying)
                attractBall.Stop();
    }

}
