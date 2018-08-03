using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SetLine : MonoBehaviour {

    //Paddle reference to spawn in line.
    public GameObject pad;
    
    //Number of paddles in line.
    public int numberPaddles;
    private Team teamInfo;
    
    //Screen width
    float screenHalfWidthInWorldUnits;
    //Movement limit
    public float xLimit;
    public float halfPlayer;

    // Use this for initialization
    void Awake () {
        if(transform.parent.name == "Player")
        {
            GameObject playerInfo = GameObject.Find("PlayerInfo");
            if (playerInfo == null) teamInfo = Resources.Load<Team>("Teams/Mexico/MexicoInfo");
            else teamInfo = playerInfo.GetComponent<TeamPickedInfo>().teamPicked;
        } else if(transform.parent.name == "NPC")
        {
            GameObject npcInfo = GameObject.Find("NpcInfo");
            if (npcInfo == null) teamInfo = Resources.Load<Team>("Teams/Argentina/ArgentinaInfo");
            else teamInfo = npcInfo.GetComponent<TeamPickedInfo>().teamPicked;
        }
        
        
        numberPaddles = GetNumberOfPaddles(gameObject.name);

        //Get screen width
        screenHalfWidthInWorldUnits = Camera.main.aspect * Camera.main.orthographicSize;
        //Spawn pads in line, given the number.
        DevidePaddlesInLine(screenHalfWidthInWorldUnits * 2, numberPaddles);
        //Get x maximum movement in the line, given the number of paddles
        xLimit = ((screenHalfWidthInWorldUnits * 2) / (numberPaddles + 1)) - 0.3f;
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
            newPaddle.GetComponent<SetAnimations>().teamPicked = teamInfo.teamName;
            newPaddle.GetComponent<SetAnimations>().SpriteSheetName = teamInfo.spriteSheetName;
            newPaddle.transform.parent = transform;
        }
    }

    int GetNumberOfPaddles(string lineType)
    {
        switch (lineType)
        {
            case "AttackLine":
                return teamInfo.attack;
            case "MidLine":
                return teamInfo.midfield;
            case "DefenseLine":
                return teamInfo.defense;
            default:
                return 1;
        }
    }
}
