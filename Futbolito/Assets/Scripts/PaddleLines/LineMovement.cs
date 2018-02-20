using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMovement : MonoBehaviour {
    
    //Line Speed. Depends on the number of players.
    public float speed;
    public bool isActive;

	void Start () {
        //Set speed of line
        SetSpeed(GetComponent<SetLine>().numberPaddles);
        //Set selection as false.
        isActive = false;
	}
	
	void Update () {
        //Line movement
        if (isActive)
        {
            //float xMov = Input.acceleration.x;
            float xMov = Input.GetAxis("Horizontal");
            float velocity = xMov * speed;
            transform.Translate(Vector2.right * velocity * Time.deltaTime);
            if (transform.position.x < -GetComponent<SetLine>().xLimit + GetComponent<SetLine>().halfPlayer)
                transform.position = new Vector2(-GetComponent<SetLine>().xLimit + GetComponent<SetLine>().halfPlayer, transform.position.y);
            if (transform.position.x > GetComponent<SetLine>().xLimit - GetComponent<SetLine>().halfPlayer)
                transform.position = new Vector2(GetComponent<SetLine>().xLimit - GetComponent<SetLine>().halfPlayer, transform.position.y);
        }
    }

    void SetSpeed(int numPlayerInLine)
    {
        switch (numPlayerInLine)
        {
            case 1:
                speed = 10f;
                break;
            case 2:
                speed = 7.5f;
                break;
            case 3:
                speed = 5f;
                break;
            case 4:
                speed = 2.5f;
                break;
            case 5:
                speed = 1f;
                break;
        }
    }
}
