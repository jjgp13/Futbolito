﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleWallPass : MonoBehaviour
{

    public string wallPassButton;
    public float wallPassForce;


    private void Start()
    {
        wallPassButton = GetComponentInParent<PaddleController>().wallPassButton;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (GetComponentInParent<PaddleController>().isLineActive)
        {
            GameObject obj = collision.gameObject;
            //If ball is inside of trigger and press wall pass
            if (obj.CompareTag("Ball") && Input.GetButtonDown(wallPassButton))
            {

                //Stop ball
                if (obj.GetComponent<Rigidbody2D>().velocity != Vector2.zero)
                {
                    obj.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    collision.gameObject.GetComponent<BallBehavior>().stopBallParticles.Play();
                }//Wall pass
                else
                {
                    if(transform.position.y >= obj.transform.position.y)
                        obj.GetComponent<Rigidbody2D>().velocity = Vector2.down * wallPassForce;
                    else
                        obj.GetComponent<Rigidbody2D>().velocity = Vector2.up * wallPassForce;
                }


                
            }
        }
    }

}