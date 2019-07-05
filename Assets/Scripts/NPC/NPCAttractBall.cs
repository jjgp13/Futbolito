using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAttractBall : MonoBehaviour
{
    public float distanceToAttract;
    public float omitAngleToAttract;
    private float attractionForce;
    private readonly float[] attractionForces = new float[3] { -0.25f, -0.5f, -0.75f };

    private List<GameObject> paddles = new List<GameObject>();
    private int npcLevel;

    private GameObject ball;

    // Start is called before the first frame update
    void Start()
    {
        npcLevel = MatchInfo._matchInfo.matchLevel - 1;
        attractionForce = 0;

        ball = GameObject.FindGameObjectWithTag("Ball");
        for (int i = 0; i < transform.childCount; i++)
        {
            paddles.Add(transform.GetChild(i).gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<NPCLineMovement>().isActive && ball != null)
        {
            //Check distances of the paddle in this line.
            //If one of them is in the distance to shoot
            bool magnetActive = CheckDistances(paddles);

            AttractBall(magnetActive);
            PlayOrStopMagnetParticles(magnetActive);
        }
        else {
            PlayOrStopMagnetParticles(false);
        }
        //If not ball, find ball
        if (ball == null) ball = GameObject.FindGameObjectWithTag("Ball");
    }


    private void AttractBall(bool isAttracting)
    {
        if (isAttracting) attractionForce = attractionForces[npcLevel];
        else attractionForce = 0;

        for (int i = 0; i < transform.childCount; i++)
        {
            //Get effector of each paddle and apply force
            paddles[i].GetComponent<PointEffector2D>().forceMagnitude = attractionForce;
            //Get animator of eahc paddle and play magnet animation
            paddles[i].GetComponent<Animator>().SetBool("Magnet", isAttracting);
        }
    }

    private bool CheckDistances(List<GameObject> paddles)
    {
        foreach (GameObject paddle in paddles)
        {
            //check if ball is in front of the paddle, given the team side
            Vector2 direction = ball.transform.position - paddle.transform.position;
            float angleToBall = Vector2.Angle(direction, paddle.transform.up);

            //Check distance to ball
            float distanceToball = Vector2.Distance(ball.transform.position, paddle.transform.position);

            //If distance to ball is less than distance to shoot and ball is in front. Shoot
            if (distanceToball < distanceToAttract && angleToBall > omitAngleToAttract)
                return true;
        }
        return false;
    }

    private void PlayOrStopMagnetParticles(bool isMagentActive)
    {
        //Get particles and play or stop it
        foreach (GameObject paddle in paddles)
        {
            if(isMagentActive)
                if(!paddle.GetComponent<NPCHitBall>().attractingParticles.isPlaying)
                    paddle.GetComponent<NPCHitBall>().attractingParticles.Play();
            else paddle.GetComponent<NPCHitBall>().attractingParticles.Stop();
        }
    }
}