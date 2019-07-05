using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCShootController : MonoBehaviour {

    public float distanceToShoot;
    public float angleToShoot;
    private List<GameObject> paddles = new List<GameObject>();
    private GameObject ball;

    private void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");

        for (int i = 0; i < transform.childCount; i++)
        {
            paddles.Add(transform.GetChild(i).gameObject);
        }
        
    }

    // Update is called once per frame
    void Update () {
        if (GetComponent<NPCLineMovement>().isActive && ball != null)
        {
            //Check distances of the paddle in this line.
            //If one of them is in the distance to shoot
            bool canShoot = CheckDistances(paddles);

            Shoot(canShoot);
        }
        //If not ball, find ball
        if (ball == null) ball = GameObject.FindGameObjectWithTag("Ball");
    }

    private void Shoot(bool isShooting)
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<Animator>().SetBool("Shoot", isShooting);
    }

    private bool CheckDistances(List<GameObject> paddles)
    {
        foreach(GameObject paddle in paddles)
        {
            //check if ball is in front of the paddle, given the team side
            Vector2 direction = ball.transform.position - paddle.transform.position;
            float angleToBall = Vector2.Angle(direction, paddle.transform.up);

            //Check distance to ball
            float distanceToball = Vector2.Distance(ball.transform.position, paddle.transform.position);

            //If distance to ball is less than distance to shoot and ball is in front. Shoot
            if (distanceToball < distanceToShoot && angleToBall < angleToShoot)
                return true;
        }
        return false;
    }
}
