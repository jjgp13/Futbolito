using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMovement : MonoBehaviour {
    
    //Line Speed. Depends on the number of players.
    public float speed;
    public float velocity;
    public bool isActive;
    private int paddlesInLine;
    private string moveAxis;
    //Could be right or left button. It depends on which lines are active
    public string holdLineButton;
    

    void Start () {
        //Set who moves this line given the controllers map;
        if(gameObject.GetComponentInParent<LinesHandler>().numberOfPlayers == 1)
        {
            moveAxis = gameObject.GetComponentInParent<LinesHandler>().defenseButtons.yAxis;
        }
        else
        {
            //Two controllers
        }

        //Set speed of line
        paddlesInLine = GetComponent<SetLine>().numberPaddles;
        SetSpeed(paddlesInLine);
        velocity = 0;
        //Set selection as false.
        isActive = false;
	}

    void LateUpdate () {
        if (isActive && !Input.GetButton(holdLineButton))
        {
            //Get Left joystick Up/Down moveAxis
            float yMov = Input.GetAxis(moveAxis);
            
            velocity = yMov * speed;
            transform.Translate(Vector3.up * velocity * Time.deltaTime); 
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
                speed = 8f;
                break;
            case 2:
                speed = 6.5f;
                break;
            case 3:
                speed = 4f;
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
