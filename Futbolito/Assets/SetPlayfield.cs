using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPlayfield : MonoBehaviour {

    //Reference to wall colliders
    public BoxCollider2D leftWall;
    public BoxCollider2D rightWall;
    public BoxCollider2D upperLeft;
    public BoxCollider2D upperRight;
    public BoxCollider2D lowerLeft;
    public BoxCollider2D lowerRight;
    public BoxCollider2D goalTriggerPlayer;
    public BoxCollider2D goalTriggerNPC;


    void Start () {
        //Set walls collider position
        //Side Walls
        leftWall.size = new Vector2(1f, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height * 1.5f, 0f)).y);
        leftWall.offset = new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).x - 0.5f, 0f);

        rightWall.size = new Vector2(1f, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height * 1.5f, 0f)).y);
        rightWall.offset = new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0f, 0f)).x + 0.5f, 0f);
        
        //UpperWalls
        upperLeft.size = new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 1f, 0f, 0f)).x, 1f);
        upperLeft.offset = new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).x, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)).y + 0.5f);

        upperRight.size = new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 1f, 0f, 0f)).x, 1f);
        upperRight.offset = new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0f, 0f)).x, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)).y + 0.5f);

        //LowerWalls
        lowerLeft.size = new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 1f, 0f, 0f)).x, 1f);
        lowerLeft.offset = new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).x, Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).y - 0.5f);

        lowerRight.size = new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 1f, 0f, 0f)).x, 1f);
        lowerRight.offset = new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0f, 0f)).x, Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).y - 0.5f);

        //Goals Triggers.
        goalTriggerPlayer.size = new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 1f, 0f, 0f)).x, 1f);
        goalTriggerPlayer.offset = new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0f, 0f)).x, Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).y - 0.75f);

        goalTriggerNPC.size = new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 1f, 0f, 0f)).x, 1f);
        goalTriggerNPC.offset = new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0f, 0f)).x, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)).y + 0.75f);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
