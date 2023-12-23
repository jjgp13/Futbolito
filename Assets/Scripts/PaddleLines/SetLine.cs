using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(SetLine))]
[RequireComponent(typeof(LineMovement))]
[RequireComponent(typeof(NPCShootController))]
[RequireComponent(typeof(NPCLineMovement))]
[RequireComponent(typeof(NPCAttractBall))]
public class SetLine : MonoBehaviour {


    [Header("References to paddles prefabs")]
    public GameObject playerPaddlePrefab;
    public GameObject npcPaddlePrefab;
    private GameObject pad;
    
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

            if (MatchInfo._matchInfo.leftControlsAssigned.Count > 0)
            {
                pad = playerPaddlePrefab;
                SetBehaviours(true);
            }
            else
            {
                pad = npcPaddlePrefab;
                SetBehaviours(false);
            }
        }

        if (transform.parent.name == "RightTeam")
        {
            teamInfo = MatchInfo._matchInfo.rightTeam;
            teamUniform = MatchInfo._matchInfo.rightTeamUniform;
            teamFormation = MatchInfo._matchInfo.rightTeamLineUp;

            if (MatchInfo._matchInfo.rightControlsAssigned.Count > 0)
            {
                pad = playerPaddlePrefab;
                SetBehaviours(true);
            }
            else
            {
                pad = npcPaddlePrefab;
                SetBehaviours(false);
            }
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
        //Check team side to rotate the paddle
        TeamSide side = GetComponentInParent<LinesHandler>().teamSide;
        float rotationZ = 0;
        if (side == TeamSide.LeftTeam) rotationZ = -90f;
        else rotationZ = 90f;

        float spawnPos = screenHeight / (numPaddles + 1);
        float iniPos = -screenHeight / 2;
        for (int i = 0; i < numberPaddles; i++)
        {
            iniPos += spawnPos;
            GameObject newPaddle = Instantiate(pad, new Vector2(transform.position.x, iniPos), Quaternion.Euler(0f,0f,rotationZ));
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

    /// <summary>
    /// Active or deactivate the scripts that will handle the lines behaviours
    /// </summary>
    /// <param name="isActive">If true, lines will be handle by player input, if not, by AI</param>
    private void SetBehaviours(bool isActive)
    {
        GetComponent<LineMovement>().enabled = isActive;
        GetComponent<NPCLineMovement>().enabled = !isActive;
        GetComponent<NPCShootController>().enabled = !isActive;
        GetComponent<NPCAttractBall>().enabled = !isActive;
    } 
}
