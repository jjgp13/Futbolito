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
    private Formation teamFormation;
    
    //Screen width
    float screenHalfWidthInWorldUnits;
    //Movement limit
    public float yLimit;
    public float halfPlayer;

    // Use this for initialization
    void Awake () {
        //Set line given it's parent
        //Take uniforms and lineup from team selected
        if (transform.parent.name == "LeftTeam")
        {
            teamInfo = MatchInfo._matchInfo.leftTeam;
            teamUniform = MatchInfo._matchInfo.leftTeamUniform;
            teamFormation = MatchInfo._matchInfo.leftTeamLineUp;
        } else if(transform.parent.name == "RightTeam")
        {
            teamInfo = MatchInfo._matchInfo.rightTeam;
            teamUniform = MatchInfo._matchInfo.rightTeamUniform;
            teamFormation = MatchInfo._matchInfo.rightTeamLineUp;
        }

        numberPaddles = GetNumberOfPaddles(gameObject.name);
        //Set line given the measures of the screen
        //Get screen width
        screenHalfWidthInWorldUnits = Camera.main.orthographicSize;
        //Spawn pads in line, given the number.
        DevidePaddlesInLine(screenHalfWidthInWorldUnits * 2, numberPaddles);
        //Get x maximum movement in the line, given the number of paddles
        yLimit = ((screenHalfWidthInWorldUnits * 2) / (numberPaddles + 1)) - 1f;
        if (numberPaddles == 1 && gameObject.name == "GKLine") yLimit = 2.25f;
        //Get size of the paddle.
        halfPlayer = pad.transform.localScale.x / 2;
    }

    /// <summary>
    /// This method distributed the paddles in a line
    /// </summary>
    /// <param name="screenWidth">The width of the screen</param>
    /// <param name="numPaddles">Numbers of paddles of distrubed in this line</param>
    void DevidePaddlesInLine(float screenHeight, int numPaddles)
    {
        float spawnPos = screenHeight / (numPaddles + 1);
        float iniPos = -screenHeight / 2;
        for (int i = 0; i < numberPaddles; i++)
        {
            iniPos += spawnPos;
            GameObject newPaddle = Instantiate(pad, new Vector2(transform.position.x, iniPos), pad.transform.rotation);
            newPaddle.GetComponent<SetAnimations>().teamPicked = teamInfo.teamName;
            newPaddle.GetComponent<SetAnimations>().uniform = teamUniform;
            newPaddle.transform.parent = transform;
        }
    }

    /// <summary>
    /// Return the numbers of paddles in certain line
    /// </summary>
    /// <param name="lineType">Attack, middle or defense, GoalKeeper return 1 by default</param>
    /// <returns>Number of paddles in this line</returns>
    int GetNumberOfPaddles(string lineType)
    {
        switch (lineType)
        {
            case "AttackLine":
                return teamFormation.attack;
            case "MidLine":
                return teamFormation.mid;
            case "DefenseLine":
                return teamFormation.defense;
            default:
                return 1;
        }
    }
}
