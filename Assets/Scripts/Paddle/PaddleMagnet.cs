using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(PointEffector2D))]
public class PaddleMagnet : MonoBehaviour
{
    private string magnetButton;
    private PointEffector2D effector;
    float attractHoldingTime = 0;

    public float attractionForce;    
    public ParticleSystem attractBall;

    private bool isMagnetOff;

    // Start is called before the first frame update
    void Start()
    {
        effector = GetComponent<PointEffector2D>();
        magnetButton = GetComponentInParent<PaddleController>().magnetButton;
        MagnetOff();
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponentInParent<PaddleController>().isLineActive)
        {
            if (Input.GetButton(magnetButton))
                MagnetOn();

            if (Input.GetButtonUp(magnetButton))
                MagnetOff();
        } else if (!isMagnetOff) MagnetOff();

    }

    private void MagnetOn()
    {
        attractHoldingTime += Time.deltaTime;
        if (attractHoldingTime > 0)
        {
            effector.forceMagnitude = attractionForce;
            if (!attractBall.isPlaying)
                attractBall.Play();
        }
    }

    public void MagnetOff()
    {
        isMagnetOff = true;
        attractHoldingTime = 0;
        GetComponent<PointEffector2D>().forceMagnitude = 0;
        attractBall.Stop();
    }

}
