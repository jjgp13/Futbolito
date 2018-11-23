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
    public float velocityLimit;

    private BallSoundsController soundC;


    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(MatchController._matchController.GetComponent<SoundMatchController>().PlayKO());
        Invoke("AddInitialVelocity", timeToWaitToStart);

        inactiveBallTime = 0;
        kickOff = false;

        soundC = GetComponent<BallSoundsController>();
        
    }

    private void FixedUpdate()
    {
        if ((rb.velocity.x > -velocityLimit && rb.velocity.x < velocityLimit && rb.velocity.y > -velocityLimit && rb.velocity.y < velocityLimit) && kickOff)
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
        float xVel = Random.Range(1f, 3f);
        float yVel = Random.Range(1f, 3f);
        switch (Random.Range(0, 4))
        {
            case 0:
                rb.AddForce(new Vector2(xVel, yVel));
                break;
            case 1:
                rb.AddForce(new Vector2(-xVel, yVel));
                break;
            case 2:
                rb.AddForce(new Vector2(-xVel, -yVel));
                break;
            case 3:
                rb.AddForce(new Vector2(xVel, -yVel));
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
        float hitForce = ShootButton._shootButton.shootForce;
        GameObject obj = other.gameObject;
        if (hitForce != 0)
        {
            if (hitForce > 2f)
            {
                MatchController._matchController.PlayBulletTimeAnimation(new Vector2(transform.position.x, transform.position.y));
                Instantiate(energyParticles, transform.position, Quaternion.identity);
            }
            float xPaddlePos = obj.transform.position.x;
            float xBallPos = transform.position.x;
            float xVel = Mathf.Abs(xBallPos - xPaddlePos);
            if (xBallPos < xPaddlePos) xVel = -xVel;
            //StartCoroutine(MatchController._matchController.BallHittedEffect());
            BallHitted(new Vector2(xVel, hitForce), other.GetContact(0).point);
            Vector3 pos = new Vector3(other.GetContact(0).point.x, other.GetContact(0).point.y);
            Instantiate(ballHit, pos, Quaternion.identity);
        }
    }

    void NPCHitBall(Collision2D other)
    {
        GameObject obj = other.gameObject;
        float shootSpeed = obj.GetComponent<NPCStats>().shootSpeed;
        float xPaddlePos = obj.transform.position.x;
        float xBallPos = transform.position.x;
        float xVel;

        if (obj.GetComponent<NPCStats>().isShooting)
        {
            xVel = Mathf.Abs(xBallPos - xPaddlePos) * shootSpeed;
            if (xBallPos < xPaddlePos) xVel = -xVel;
            float yVel = -shootSpeed;
            //Add force
            BallHitted(new Vector2(xVel, yVel), other.GetContact(0).point);
            Vector3 pos = new Vector3(other.GetContact(0).point.x, other.GetContact(0).point.y);
            Instantiate(ballHit, pos, Quaternion.identity);
        }
    }

    public void BallHitted(Vector2 force, Vector2 point)
    {
        soundC.PlaySound(soundC.paddleHit);
        rb.AddForceAtPosition(force, point, ForceMode2D.Impulse);
        //rb.AddForce(force, ForceMode2D.Impulse);
    }
    
}