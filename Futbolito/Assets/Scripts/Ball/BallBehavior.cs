using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehavior : MonoBehaviour {

    Rigidbody2D rb;
    public GameObject ballExplosion;
    public float timeToWaitToStart;
    private float ballInactive;
    private bool kickOff;


    [Range(0, 50)]
    public float initalBallForce;
    [Header("Values between 0 and 1")]
    public Vector2 DecreaseFactor;

    private BallSoundsController soundC;

    private ShootButton shootBtn;
    private HoldButton holdBtn;

    [Range(0f, 1f)]
    public float wallHitDrag;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(MatchController._matchController.GetComponent<SoundMatchController>().PlayKO());
        Invoke("AddInitialVelocity", timeToWaitToStart);

        ballInactive = 0;
        kickOff = false;

        soundC = GetComponent<BallSoundsController>();
    }

    private void FixedUpdate()
    {
        if(transform.parent != null)
        {
            float velOnRelease = transform.parent.GetComponentInParent<LineMovement>().velocity;
            if (!holdBtn.isHolding || holdBtn.empty)
            {
                transform.parent = null;
                rb.velocity = new Vector2(velOnRelease, 0f);
            }
        }
        else
        {
            if (rb.velocity == Vector2.zero && kickOff) ballInactive += Time.deltaTime;
            else ballInactive = 0;

            if(ballInactive > 3f)
            {
                MatchController._matchController.SpawnBall();
                Destroy(gameObject);
            }
        }
    }

    void AddInitialVelocity()
    {
        shootBtn = GameObject.Find("ShootBtn").GetComponent<ShootButton>();
        holdBtn = GameObject.Find("HoldBtn").GetComponent<HoldButton>();
        Vector2 initialVel = new Vector2(Random.Range(-initalBallForce, initalBallForce), Random.Range(-initalBallForce, initalBallForce));
        rb.AddForce(initialVel);
        kickOff = true;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "PlayerPaddle") PlayerHitBall(other.gameObject);
        if (other.gameObject.tag == "NPCPaddle") NPCHitBall(other.gameObject);
        if (other.gameObject.tag == "Wall") BallHitAgainstWall(other.gameObject);
    }

    void PlayerHitBall(GameObject obj)
    {
        bool holding = holdBtn.isHolding;
        if (holding)
        {
            rb.velocity = Vector2.zero;
            gameObject.transform.SetParent(obj.transform);
        }
        else
        {
            float yForce = obj.GetComponent<PlayerAnimationController>().yForce;
            if (yForce != 0)
            {
                float xForce = obj.GetComponent<PlayerAnimationController>().xForce;
                float xPaddlePos = obj.transform.position.x;
                float xBallPos = transform.position.x;
                float xVel = Mathf.Abs(xBallPos - xPaddlePos) * xForce;
                if (xBallPos < xPaddlePos) xVel = -xVel;
                //Add force
                BallHitted(new Vector2(xVel, yForce));
            }
            else
            {
                BallHitPadWithNoState();
            }
        }
    }

    void NPCHitBall(GameObject obj)
    {
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
            BallHitted(new Vector2(xVel * 1.5f, yVel));
        }
        else
        {
            BallHitPadWithNoState();
        }
    }

    public void BallHitAgainstWall(GameObject obj)
    {
        soundC.PlaySound(soundC.againstWall);
        Vector2 vel = rb.velocity;
        vel *= wallHitDrag;
        rb.velocity = vel;
    }

    public void BallHitPadWithNoState()
    {
        soundC.PlaySound(soundC.againstPaddle);
        Vector2 newVel = rb.velocity;
        newVel.x *= DecreaseFactor.x;
        newVel.y *= DecreaseFactor.y;
        rb.velocity = newVel;
    }

    public void BallHitted(Vector2 force)
    {
        soundC.PlaySound(soundC.paddleHit);
        rb.AddForce(force);
        rb.AddTorque(force.x, ForceMode2D.Impulse);
    }
}