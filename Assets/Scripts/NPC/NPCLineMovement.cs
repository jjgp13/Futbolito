using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLineMovement : MonoBehaviour {

    public GameObject ball;
    public float speed;
    public bool isActive;
    public float nearDistance;
    private int paddlesInLine;
    private List<GameObject> paddles = new List<GameObject>();

    // Use this for initialization
    void Start () {
        ball = GameObject.Find("Ball");
        //Set speed of line
        paddlesInLine = GetComponent<SetLine>().numberPaddles;
        SetLineSpeed(paddlesInLine);

        //Populate list of child paddles
        for (int i = 0; i < transform.childCount; i++)
            paddles.Add(transform.GetChild(i).gameObject);
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        if (isActive && ball != null)
        {
            //Get nearest Paddle
            GameObject nearestChild = GetNearestPaddle(paddles);

            if (ball.transform.position.y > nearestChild.transform.position.y) transform.Translate(Vector2.up * speed * Time.deltaTime);
            if (ball.transform.position.y < nearestChild.transform.position.y) transform.Translate(-Vector2.up * speed * Time.deltaTime);

            if (transform.position.y < -GetComponent<SetLine>().yLimit + GetComponent<SetLine>().halfPlayer)
                transform.position = new Vector2(transform.position.x, -GetComponent<SetLine>().yLimit + GetComponent<SetLine>().halfPlayer);
            if (transform.position.y > GetComponent<SetLine>().yLimit - GetComponent<SetLine>().halfPlayer)
                transform.position = new Vector2(transform.position.x, GetComponent<SetLine>().yLimit - GetComponent<SetLine>().halfPlayer);
        }

        if (ball == null) ball = GameObject.FindGameObjectWithTag("Ball");
    }

    /// <summary>
    /// Given the number of paddles in this line. Calculate the one who is nearest to the ball
    /// </summary>
    /// <param name="paddlesInThisLine">Child paddles</param>
    /// <returns>Nearest paddle to the ball</returns>
    GameObject GetNearestPaddle(List<GameObject> paddlesInThisLine)
    {
        GameObject nearChild;
        nearChild = paddlesInThisLine[0];
        nearDistance = Vector2.Distance(ball.transform.position, nearChild.transform.position);
        for (int i = 1; i < paddlesInThisLine.Count; i++)
        {
            float distance = Vector2.Distance(ball.transform.position, paddlesInThisLine[i].transform.position);
            if (distance < nearDistance)
            {
                nearDistance = distance;
                nearChild = paddlesInThisLine[i];
            }
        }


        return nearChild;
    }

    void SetLineSpeed(int numPlayerInLine)
    {
        switch (numPlayerInLine)
        {
            case 1:
                speed = 4f;
                break;
            case 2:
                speed = 3.25f;
                break;
            case 3:
                speed = 2.5f;
                break;
            case 4:
                speed = 1.75f;
                break;
            case 5:
                speed = 1f;
                break;
        }

        if (MatchInfo._matchInfo.matchLevel == 1) speed -= 0.5f;
        if (MatchInfo._matchInfo.matchLevel == 3) speed += 0.5f;
    }
}
