using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMovement : MonoBehaviour {
    
    //Line Speed. Depends on the number of players.
    public float speed;
    public float velocity;
    public bool isActive;
    private int paddlesInLine;
    private Vector3 phoneIniAcc, accDifference;

    private void Awake()
    {
        phoneIniAcc = Input.acceleration;
    }

    void Start () {
        //Set speed of line
        paddlesInLine = GetComponent<SetLine>().numberPaddles;
        SetSpeed(paddlesInLine);
        velocity = 0;
        //Set selection as false.
        isActive = false;
	}
	
	void LateUpdate () {
        if (isActive)
        {
            accDifference = phoneIniAcc - Input.acceleration;
            float yMov = accDifference.y;
            
            velocity = yMov * speed;
            transform.Translate(-Vector3.up * velocity * Time.deltaTime); 
            if (transform.position.y < -GetComponent<SetLine>().yLimit + GetComponent<SetLine>().halfPlayer)
                transform.position = new Vector2(transform.position.x, -GetComponent<SetLine>().yLimit + GetComponent<SetLine>().halfPlayer);
            if (transform.position.y > GetComponent<SetLine>().yLimit - GetComponent<SetLine>().halfPlayer)
                transform.position = new Vector2(transform.position.x, GetComponent<SetLine>().yLimit - GetComponent<SetLine>().halfPlayer);
        }
    }

    void SetSpeed(int numPlayerInLine)
    {
        switch (numPlayerInLine)
        {
            case 1:
                speed = 17.5f;
                break;
            case 2:
                speed = 15f;
                break;
            case 3:
                speed = 12.5f;
                break;
            case 4:
                speed = 10f;
                break;
            case 5:
                speed = 7.5f;
                break;
        }
    }
}
