using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehavior : MonoBehaviour {

    Rigidbody2D rb;
    public ParticleSystem ballExplosion;
    public ParticleSystem ballHitParticles;
    public ParticleSystem stopBallParticles;
    public GameObject energyParticles;


    public float timeToWaitToStart;
    private float inactiveBallTime;
    private bool kickOff;

    [Header("Restarting ball values")]
    public int timeInactiveToRespawn;
    public float minVelocityLimit;
    public float iniMinForce, iniMaxForce;

    private BallSoundsController soundC;


    // Use this for initialization
    void Start () {
        //Set sprite of the ball selected
        GetComponent<SpriteRenderer>().sprite = MatchInfo._matchInfo.ballSelected;
        
        //Get Rigibody
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(MatchController._matchController.GetComponent<SoundMatchController>().PlayKO());
        Invoke("AddInitialVelocity", timeToWaitToStart);

        inactiveBallTime = 0;
        kickOff = false;

        soundC = GetComponent<BallSoundsController>();
    }

    private void FixedUpdate()
    {
        Debug.Log(rb.velocity.magnitude);
        if ((rb.velocity.x > -minVelocityLimit && rb.velocity.x < minVelocityLimit && rb.velocity.y > -minVelocityLimit && rb.velocity.y < minVelocityLimit) && kickOff)
        {
            inactiveBallTime += Time.deltaTime;
            if (timeInactiveToRespawn - inactiveBallTime <= 3)
            {
                MatchController._matchController.timeInactiveBallPanel.SetActive(true);
                MatchController._matchController.restartingBallTimeText.text = "RESTARTING BALL IN: " + (timeInactiveToRespawn - inactiveBallTime).ToString("0");
            }
        }
        else
        {
            MatchController._matchController.timeInactiveBallPanel.SetActive(false);
            inactiveBallTime = 0;
        }


        if (inactiveBallTime >= timeInactiveToRespawn)
        {
            MatchController._matchController.timeInactiveBallPanel.SetActive(false);
            MatchController._matchController.SpawnBall();
            Destroy(gameObject);
        }       
    }

    /// <summary>
    /// Add initial velocity when game starts
    /// </summary>
    void AddInitialVelocity()
    {
        float xVel = Random.Range(iniMinForce, iniMaxForce);
        float yVel = Random.Range(iniMinForce, iniMaxForce); 
        switch (Random.Range(0, 4))
        {
            case 0:
                rb.AddForce(new Vector2(xVel, yVel), ForceMode2D.Impulse);
                break;
            case 1:
                rb.AddForce(new Vector2(-xVel, yVel), ForceMode2D.Impulse);
                break;
            case 2:
                rb.AddForce(new Vector2(-xVel, -yVel), ForceMode2D.Impulse);
                break;
            case 3:
                rb.AddForce(new Vector2(xVel, -yVel), ForceMode2D.Impulse);
                break;
        }
        kickOff = true;
        MatchController._matchController.ballInGame = true;
    }

    /// <summary>
    /// Check object collision and play sound given the object
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionEnter2D(Collision2D other)
    {
        //if (other.gameObject.tag == "PlayerPaddle" || other.gameObject.tag == "NPCPaddle")
        //    soundC.PlaySound(soundC.paddleHit);
        //if (other.gameObject.tag == "Wall")
        //    soundC.PlaySound(soundC.againstWall);
    }
    
}