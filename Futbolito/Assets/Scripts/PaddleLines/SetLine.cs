using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLine : MonoBehaviour {

    //Number of paddles in line.
    public int numberPaddles;
    //Paddle reference to spawn in line.
    public GameObject pad;
    //Screen width
    float screenHalfWidthInWorldUnits;
    //Movement limit
    public float xLimit;
    public float halfPlayer;

    // Use this for initialization
    void Start () {
        //Get screen width
        screenHalfWidthInWorldUnits = Camera.main.aspect * Camera.main.orthographicSize;
        //Spawn pads in line, given the number.
        DevidePaddlesInLine(screenHalfWidthInWorldUnits * 2, numberPaddles);
        //Get x maximum movement in the line, given the number of paddles
        xLimit = (screenHalfWidthInWorldUnits * 2) / (numberPaddles + 1);
        //Get size of the paddle.
        halfPlayer = pad.transform.localScale.x / 2;
    }

    void DevidePaddlesInLine(float screenWidth, int numPaddles)
    {
        float spawnPos = screenWidth / (numPaddles + 1);
        float iniPos = -screenWidth / 2;
        for (int i = 0; i < numberPaddles; i++)
        {
            iniPos += spawnPos;
            GameObject newPaddle = Instantiate(pad, new Vector2(iniPos, transform.position.y), Quaternion.identity);
            newPaddle.transform.parent = transform;
        }
    }
}
