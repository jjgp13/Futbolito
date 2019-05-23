using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehavior : MonoBehaviour {

    Rigidbody2D rb;
    public ParticleSystem ballExplosion;
    public ParticleSystem ballHit;
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "PlayerPaddle") PlayerHitBall(other);
        if (other.gameObject.tag == "NPCPaddle") NPCHitBall(other);
    }

    void PlayerHitBall(Collision2D other)
    {
        float hitForce = other.gameObject.GetComponent<PlayerAnimationController>().shootForce;
        GameObject obj = other.gameObject;
        if (hitForce != 0)
        {
            if (hitForce > 2f)
            {
                MatchController._matchController.PlayBulletTimeAnimation(new Vector2(transform.position.x, transform.position.y));
                Instantiate(energyParticles, transform.position, Quaternion.identity);
            }
            if (hitForce < 1) hitForce = 1;
            float yVel = (Mathf.Abs(transform.position.y - obj.transform.position.y) * hitForce * 1.75f);
            if (transform.position.y < obj.transform.position.y) yVel = -yVel;
            //StartCoroutine(MatchController._matchController.BallHittedEffect());
            ContactPoint2D point2D = other.GetContact(0);
            BallHitted(new Vector2(hitForce, yVel), point2D.point);
            Vector3 pos = new Vector3(other.GetContact(0).point.x, other.GetContact(0).point.y);
            Instantiate(ballHit, pos, Quaternion.identity);
        }
    }

    void NPCHitBall(Collision2D other)
    {
        GameObject obj = other.gameObject;
        NPCStats level = obj.transform.parent.parent.GetComponent<NPCStats>();
        float shootSpeed = level.shootSpeed;
        float yPaddlePos = obj.transform.position.y;
        float yBallPos = transform.position.y;
        float yVel;
        
        if (obj.GetComponent<Animator>().GetBool("Shoot"))
        {
            yVel = (Mathf.Abs(yBallPos - yPaddlePos) * shootSpeed * 1.75f);
            if (yBallPos < yPaddlePos) yVel = -yVel;
            float xVel = -shootSpeed;
            //Add force
            Vector3 pos = new Vector3(other.GetContact(0).point.x, other.GetContact(0).point.y);
            BallHitted(new Vector2(xVel, yVel), pos);
            
            Instantiate(ballHit, pos, Quaternion.identity);
        }
    }

    public void BallHitted(Vector2 force, Vector2 hitPostion)
    {
        soundC.PlaySound(soundC.paddleHit);
        rb.AddForceAtPosition(force, hitPostion,ForceMode2D.Impulse);
    }
    
}