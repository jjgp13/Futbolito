using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(PointEffector2D))]
public class PaddleMagnet : MonoBehaviour
{
    private string attractButton;
    private PointEffector2D effector;

    public float attractionForce;
    float attractHoldingTime = 0;

    public ParticleSystem attractBall;

    // Start is called before the first frame update
    void Start()
    {
        effector = GetComponent<PointEffector2D>();
        //attractButton = transform.parent.GetComponent<PlayerAnimationController>().linesHandler.defenseButtons.attractButton;
        attractBall.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        CheckMagnet();
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
