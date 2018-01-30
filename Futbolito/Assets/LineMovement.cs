using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMovement : MonoBehaviour {
    
    //Line Speed. Depends on the number of players.
    public float speed;
    //Number of paddles in line.
    public int numberPaddles;
    //Paddle reference to spawn in line.
    public GameObject pad;
    //Screen width
    float screenHalfWidthInWorldUnits;


	
	void Start () {

        screenHalfWidthInWorldUnits = Camera.main.aspect * Camera.main.orthographicSize;
        print(screenHalfWidthInWorldUnits);
	}
	
	void Update () {
        //Line movement
        float xMov = Input.acceleration.x;
        float velocity = xMov * speed;
        transform.Translate(Vector2.right * velocity * Time.deltaTime);


	}
}
