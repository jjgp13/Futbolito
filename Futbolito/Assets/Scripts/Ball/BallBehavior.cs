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

    public float slowDownFactor;
    public float slowDownTime;
    float timer = 0;

    [Header("Restarting ball values")]
    public int timeInactiveToRespawn;
    public float velocityLimit;

    [Range(0, 50)]
    public float initalBallForce;
    [Header("Values between 0 and 1")]
    public Vector2 DecreaseFactor;

    private BallSoundsController soundC;

    [Range(0f, 1f)]
    public float wallHitDrag;

    [Header("Camera references")]
    public Camera mainCamera;
    private Vector3 camaraIniPos;
    public float cameraMinSize;
    public float cameraMaxSize;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(MatchController._matchController.GetComponent<SoundMatchController>().PlayKO());
        Invoke("AddInitialVelocity", timeToWaitToStart);

        inactiveBallTime = 0;
        kickOff = false;

        soundC = GetComponent<BallSoundsController>();

        mainCamera = FindObjectOfType<Camera>();
        camaraIniPos = mainCamera.transform.position;
    }

    void Update()
    {
        if(Time.timeScale < 1f && !MatchController._matchController.gameIsPaused)
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= slowDownTime)
            {
                Time.timeScale = 1f;
                mainCamera.orthographicSize = 6.4f;
                mainCamera.transform.position = camaraIniPos;
            }   
        }
        else
        {
            timer = 0;
        }
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
        Vector2 initialVel = new Vector2(Random.Range(-initalBallForce, initalBallForce) * 2, Random.Range(-initalBallForce, initalBallForce));
        rb.AddForce(initialVel);
        kickOff = true;
        MatchController._matchController.ballInGame = true;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "PlayerPaddle") PlayerHitBall(other);
        if (other.gameObject.tag == "NPCPaddle") NPCHitBall(other);
        if (other.gameObject.tag == "Wall") BallHitAgainstWall(other.gameObject);
    }

    void PlayerHitBall(Collision2D other)
    {
        GameObject obj = other.gameObject;
        
        float yForce = obj.GetComponent<PlayerAnimationController>().yForce;
        if (yForce != 0)
        {
            if (yForce > 100f) StopTimeOnBallHit();
            float xForce = obj.GetComponent<PlayerAnimationController>().xForce;
            float xPaddlePos = obj.transform.position.x;
            float xBallPos = transform.position.x;
            float xVel = Mathf.Abs(xBallPos - xPaddlePos) * xForce;
            if (xBallPos < xPaddlePos) xVel = -xVel;
            //Add force
            BallHitted(new Vector2(xVel, yForce));
            Vector3 pos = new Vector3(other.GetContact(0).point.x, other.GetContact(0).point.y);
            Instantiate(ballHit, pos, Quaternion.identity);
        }
        else
        {
            BallHitPadWithNoState(other.GetContact(0).normal.y, obj.tag);
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
            BallHitted(new Vector2(xVel, yVel));
            Vector3 pos = new Vector3(other.GetContact(0).point.x, other.GetContact(0).point.y);
            Instantiate(ballHit, pos, Quaternion.identity);
        }
        else
        {
            BallHitPadWithNoState(other.GetContact(0).normal.y, obj.tag);
        }
    }

    public void BallHitAgainstWall(GameObject obj)
    {
        soundC.PlaySound(soundC.againstWall);
        Vector2 vel = rb.velocity;
        vel *= wallHitDrag;
        rb.velocity = vel;
    }

    public void BallHitPadWithNoState(float normal, string tag)
    {
        if (normal > 0 && tag == "PlayerPaddle") DecreaseBallVel();
        else if(normal < 0 && tag == "NPCPaddle") DecreaseBallVel();
    }

    void DecreaseBallVel()
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

    void StopTimeOnBallHit()
    {
        Instantiate(energyParticles, transform.position, Quaternion.identity);
        Time.timeScale = slowDownFactor;
        mainCamera.orthographicSize = 1;
        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, -20f);
    }
}