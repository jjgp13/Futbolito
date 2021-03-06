﻿using UnityEngine;
using UnityEngine.UI;

public class PlayerAnimationController : MonoBehaviour {

    public Animator animatorController;
    public float attractionForce;

    public ParticleSystem attractBall;
    public ParticleSystem chargingShoot;

    private void Start()
    {
        attractBall.Stop();
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (gameObject.GetComponentInParent<LineMovement>().isActive)
        {
            if (ShootButton._shootButton.isShooting)
            {
                if(!chargingShoot.isPlaying) chargingShoot.Play();
                var emission = chargingShoot.emission;
                emission.rateOverTime = ShootButton._shootButton.holdingTime * 50f;

                animatorController.SetBool("touching", true);
                animatorController.SetFloat("timeTouching", ShootButton._shootButton.holdingTime);
            }
            else
            {
                if (chargingShoot.isPlaying)
                {
                    chargingShoot.Stop();
                    var emission = chargingShoot.emission;
                    emission.rateOverTime = 0;
                }

                animatorController.SetBool("touching", false);
                animatorController.SetFloat("timeTouching", ShootButton._shootButton.holdingTime);
            }

            if (HoldButton._holdButton.isHolding)
            {
                if(HoldButton._holdButton.availableTime > 0 && !HoldButton._holdButton.empty)
                {
                    GetComponent<PointEffector2D>().forceMagnitude = attractionForce;
                    if(!attractBall.isPlaying)
                        attractBall.Play();
                }
            }
            else
            {
                GetComponent<PointEffector2D>().forceMagnitude = 0;
                attractBall.Stop();
            }
        }
        else
        {
            if (attractBall.isPlaying) attractBall.Stop();
            if (chargingShoot.isPlaying) chargingShoot.Stop();
            animatorController.SetBool("touching", false);
            animatorController.SetFloat("timeTouching", 0);
        }
    }

    public void ResetShootForce()
    {
        ShootButton._shootButton.shootForce = 0;
    }

}
