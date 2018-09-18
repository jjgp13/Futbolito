using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SetLine : MonoBehaviour {

    //Paddle reference to spawn in line.
    public GameObject pad;
    
    //Number of paddles in line.
    public int numberPaddles;
    private Team teamInfo;
    private string teamUniform;
    
    //Screen width
    float screenHalfWidthInWorldUnits;
    //Movement limit
    public float xLimit;
    public float halfPlayer;

    // Use this for initialization
    void Awake () {
        MatchInfo matchInfo = GameObject.Find("MatchInfo").GetComponent<MatchInfo>();

        if (transform.parent.name == "Player")
        {
            teamInfo = Resources.Load<Team>("Teams/" + matchInfo.playerTeam.teamName + "/" + matchInfo.playerTeam.teamName);
            teamUniform = matchInfo.playerUniform;
        } else if(transform.parent.name == "NPC")
        {
            teamInfo = Resources.Load<Team>("Teams/" + matchInfo.comTeam.teamName + "/" + matchInfo.comTeam.teamName);
            teamUniform = matchInfo.comUniform;
        }
        
        
        numberPaddles = GetNumberOfPaddles(gameObject.name);

        //Get screen width
        screenHalfWidthInWorldUnits = Camera.main.aspect * Camera.main.orthographicSize;
        //Spawn pads in line, given the number.
        DevidePaddlesInLine(screenHalfWidthInWorldUnits * 2, numberPaddles);
        //Get x maximum movement in the line, given the number of paddles
        xLimit = ((screenHalfWidthInWorldUnits * 2) / (numberPaddles + 1)) - 0.3f;
        if (numberPaddles == 1 && gameObject.name == "GKLine") xLimit = 1.75f;
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
            newPaddle.GetComponent<SetAnimations>().uniform = teamUniform;
            newPaddle.transform.parent = transform;
        }
    }

    int GetNumberOfPaddles(string lineType)
    {
        switch (lineType)
        {
            case "AttackLine":
                return teamInfo.teamFormation.attack;
            case "MidLine":
                return teamInfo.teamFormation.mid;
            case "DefenseLine":
                return teamInfo.teamFormation.defense;
            default:
                return 1;
        }
    }
}
