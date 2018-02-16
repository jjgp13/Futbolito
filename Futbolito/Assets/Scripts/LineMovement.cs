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
        DevidePaddlesInLine(screenHalfWidthInWorldUnits * 2, numberPaddles);
	}
	
	void Update () {
        //Line movement
        //float xMov = Input.acceleration.x;
        float xMov = Input.GetAxis("Horizontal");
        float velocity = xMov * speed;
        transform.Translate(Vector2.right * velocity * Time.deltaTime);
	}

    void DevidePaddlesInLine(float screenWidth, int numPaddles){
        float spawnPos = screenWidth / numPaddles;
        float iniPos = -screenWidth / 2;
        for (int i = 0; i < numberPaddles; i++)
        {
            GameObject newPaddle = (GameObject)Instantiate(pad, new Vector2(iniPos, transform.position.y), Quaternion.identity);
            newPaddle.transform.parent = transform;
            iniPos += spawnPos;
        }
    }
}
