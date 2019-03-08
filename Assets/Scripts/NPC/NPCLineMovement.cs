using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLineMovement : MonoBehaviour {

    public GameObject ball;
    public float speed;
    public bool isActive;
    public float nearDistance;
    private int paddlesInLine;

    // Use this for initialization
    void Start () {
        ball = GameObject.Find("Ball");
        //Set speed of line
        paddlesInLine = GetComponent<SetLine>().numberPaddles;
        SetLineSpeed(paddlesInLine);
    }
	
	// Update is called once per frame
	void Update () {
        if (isActive && ball != null)
        {
            //Get nearest Child
            GameObject nearChild = transform.GetChild(0).gameObject;
            nearDistance = Vector2.Distance(ball.transform.position, transform.GetChild(0).transform.position);
            for (int i = 1; i < transform.childCount; i++)
            {
                float nextDistance = Vector2.Distance(ball.transform.position, transform.GetChild(i).transform.position);
                if (nearDistance > nextDistance)
                {
                    nearDistance = nextDistance;
                    nearChild = transform.GetChild(i).gameObject;
                }
            }

            if (ball.transform.position.y > nearChild.transform.position.y) transform.Translate(Vector2.up * speed * Time.deltaTime);
            if (ball.transform.position.y < nearChild.transform.position.y) transform.Translate(Vector2.up * -speed * Time.deltaTime);

            if (transform.position.y < -GetComponent<SetLine>().yLimit + GetComponent<SetLine>().halfPlayer)
                transform.position = new Vector2(transform.position.x, -GetComponent<SetLine>().yLimit + GetComponent<SetLine>().halfPlayer);
            if (transform.position.y > GetComponent<SetLine>().yLimit - GetComponent<SetLine>().halfPlayer)
                transform.position = new Vector2(transform.position.x, GetComponent<SetLine>().yLimit - GetComponent<SetLine>().halfPlayer);
        }

        if (ball == null) ball = GameObject.FindGameObjectWithTag("Ball");
    }


    void SetLineSpeed(int numPlayerInLine)
    {
        switch (numPlayerInLine)
        {
            case 1:
                speed = 1.5f;
                break;
            case 2:
                speed = 1.25f;
                break;
            case 3:
                speed = 1f;
                break;
            case 4:
                speed = 0.75f;
                break;
            case 5:
                speed = 0.5f;
                break;
        }

        if (MatchInfo._matchInfo.matchLevel == 1) speed -= 0.25f;
        if (MatchInfo._matchInfo.matchLevel == 3) speed += 0.25f;
    }
}
