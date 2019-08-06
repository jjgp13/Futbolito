using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Type of player
public enum PlayerNumber
{
    Player1,
    Player2,
    Player3,
    Player4,
    COM
};

public enum ControlType
{
    Automatic,
    Manual
};

public enum TeamSide
{
    LeftTeam,
    RightTeam
};

public class LinesHandler : MonoBehaviour {

    [Header("Player side")]
    //Left or Right player
    public TeamSide teamSide;
    public ControlType controlType;
    private int lineIndex;

    [Header("Player's count on this team")]
    public int numberOfPlayers;

    //Default controller to map strings for buttons
    [Header("Defender controlls")]
    public ControlMapping defenseButtons;
    [Header("Attacker controlls")]
    public ControlMapping attackerButtons;
    
    private readonly float[] linesActiveBallLimit = new float[3];

    //Reference to the active ball
    private GameObject ball;

    [Header("Reference to child gameobjects (lines and indicators)")]
    //Reference to lines
    public GameObject[] lines = new GameObject[4];
    public bool[] activeLines = new bool[4];

    //reference to indicators to see which line is active
    public SpriteRenderer[] linesIndicators = new SpriteRenderer[4];
    public Sprite inactiveLineSprite;
    public Sprite activeLineSprite;

    private void Awake()
    {
        if(teamSide == TeamSide.LeftTeam && MatchInfo._matchInfo.leftControlsAssigned.Count > 0)
            AssignControlls(teamSide);
        if (teamSide == TeamSide.RightTeam && MatchInfo._matchInfo.rightControlsAssigned.Count > 0)
            AssignControlls(teamSide);
    }

    private void Start()
    {
        //Get the active ball
        ball = GameObject.FindGameObjectWithTag("Ball");
        //When game start, check the number of controlls in each team
        //If the team is not handle by AI, check type of controls select
        if (teamSide == TeamSide.LeftTeam)
            SetLinesActiveLimits(-8.3f, -3f, 3f);


        if (teamSide == TeamSide.RightTeam)
            SetLinesActiveLimits(8.3f, 3f, -3f);

        //If two players are in the same team. Hold button is assigned automic
        if (MatchInfo._matchInfo.leftControlsAssigned.Count == 2)
            AssignHoldButtonToLinesTwoPlayers(teamSide);
        else
        {
            //If not, hold button should be changed every time lines are activated
            //Only the goalkeeper and the strikers will always have the same hold button assigned
            //Left team
            if (teamSide == TeamSide.LeftTeam)
            {
                lines[0].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[0].leftButton;//Goalkeaper
                lines[1].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[0].rightButton;//Defense
                lines[2].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[0].leftButton;//Mid
                lines[3].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[0].rightButton;//Strikers
            }
            //Right team
            if (teamSide == TeamSide.RightTeam)
            {
                lines[1].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.rightControllers[0].leftButton;//Defense
                lines[2].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.rightControllers[0].rightButton;//Mid
                lines[3].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.rightControllers[0].leftButton;//Strikers
                lines[0].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.rightControllers[0].rightButton;//Goalkeaper
            }
        }


        //if (controlType == ControlType.Manual)
        //{
        //    lineIndex = 1;
        //    ActivateLines(activeLines);
        //}
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        if (controlType == ControlType.Automatic) AutomaticControls();
        //Manual controlls
        //Still implementing
        //if (controlType == ControlType.Manual) ManualControls()
    }

    private void AutomaticControls()
    {
        //If there's a ball in the field, get the two nearest behind ball
        if (ball != null)
        {
            if (teamSide == TeamSide.LeftTeam) GetClosetsLinesLeftSide();
            if (teamSide == TeamSide.RightTeam) GetClosetsLinesRightSide();
        }//If not get the reference to the ball
        else ball = GameObject.FindGameObjectWithTag("Ball");
    }

    private void ManualControls(int playersInTeam)
    {
        if(playersInTeam == 1)
        {
            //Change lines to left
            if (Input.GetButtonDown(defenseButtons.leftButton))
                if (lineIndex > 0) lineIndex--;
            //Change lines to right
            if (Input.GetButtonDown(defenseButtons.rightButton))
                if (lineIndex < 2) lineIndex++;
            //Active line given index
            ManualActiveLines(lineIndex);
        }
    }


    /// <summary>
    /// Given ball x position in field, active lines
    /// </summary>
    private void GetClosetsLinesLeftSide()
    {
        float ballPos = ball.transform.position.x;
        bool ballInMiddle = false;

        if (ballPos < linesActiveBallLimit[0])
            activeLines = new bool[] { true, false, false, false };
        else if (ballPos < linesActiveBallLimit[1])
            activeLines = new bool[] { true, true, false, false };
        else if (ballPos > linesActiveBallLimit[2])
            activeLines = new bool[] { false, false, true, true };
        else
        {
            activeLines = new bool[] { false, true, true, false };
            ballInMiddle = true;
        }
            

        ActivateLines(activeLines);
        ChangeLineIndicator(activeLines);
        AssignHoldButtonToLinesSinglePlyers(ballInMiddle, teamSide);
    }

    private void GetClosetsLinesRightSide()
    {
        float ballPos = ball.transform.position.x;
        bool ballInMiddle = false;

        if (ballPos > linesActiveBallLimit[0])
            activeLines = new bool[] { true, false, false, false };
        else if (ballPos > linesActiveBallLimit[1])
            activeLines = new bool[] { true, true, false, false };
        else if (ballPos < linesActiveBallLimit[2])
            activeLines = new bool[] { false, false, true, true };
        else
        {
            activeLines = new bool[] { false, true, true, false };
            ballInMiddle = true;
        }
            

        ActivateLines(activeLines);
        ChangeLineIndicator(activeLines);
        AssignHoldButtonToLinesSinglePlyers(ballInMiddle, teamSide);
    }

    /// <summary>
    /// Select a line to activate
    /// </summary>
    /// <param name="conf">Bool array with lines to active 0 is goalkeeper - 3 are strikers</param>
    void ActivateLines(bool[] conf)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if(lines[i].GetComponent<LineMovement>() != null)
                lines[i].GetComponent<LineMovement>().isActive = conf[i];
            if (lines[i].GetComponent<NPCLineMovement>() != null)
                lines[i].GetComponent<NPCLineMovement>().isActive = conf[i];
            
            for (int j = 0; j < lines[i].transform.childCount; j++)
            {
                lines[i].transform.GetChild(j).GetComponent<Animator>().SetBool("Active", conf[i]);
                //If magnet is on, close it
                //lines[i].transform.GetChild(j).transform.GetComponentInChildren<PaddleMagnet>().MagnetOff();
            }
                
        }
        //Circle indicators of active lines
        for(int i = 0; i < linesIndicators.Length; i++)
        {
            if(conf[i]) linesIndicators[i].sprite = activeLineSprite;
            else linesIndicators[i].sprite = inactiveLineSprite;
        }
    }

    /// <summary>
    /// Set the ball limits to active certain lines
    /// </summary>
    /// <param name="goalKeeperLimit">Ball position to active only goalkeeper</param>
    /// <param name="gkAndDefenseLimit">Ball position to active goalkeeper and defense</param>
    /// <param name="attackAndMidLimit">Ball position to active strikers and midlefields</param>
    private void SetLinesActiveLimits(float goalKeeperLimit, float gkAndDefenseLimit, float attackAndMidLimit)
    {
        linesActiveBallLimit[0] = goalKeeperLimit;
        linesActiveBallLimit[1] = gkAndDefenseLimit;
        linesActiveBallLimit[2] = attackAndMidLimit;
    }

    /// <summary>
    /// If player choose manual control, it will active lines with L and R buttons 
    /// </summary>
    /// <param name="index"></param>
    private void ManualActiveLines(int index)
    {
        switch (index)
        {
            case 0:
                ActivateLines(new bool[] { true, true, false, false });
                break;
            case 1:
                ActivateLines(new bool[] { false, true, true, false });
                break;
            case 2:
                ActivateLines(new bool[] { false, false, true, true });
                break;
        }
    }

    private void AssignControlls(TeamSide side)
    {
        int controlsCount;
        List<ControlMapping> controlList;
        if (side == TeamSide.LeftTeam)
        {
            controlsCount = MatchInfo._matchInfo.leftControlsAssigned.Count;
            controlList = MatchInfo._matchInfo.leftControllers;
        }
        else
        {
            controlsCount = MatchInfo._matchInfo.rightControlsAssigned.Count;
            controlList = MatchInfo._matchInfo.rightControllers;
        }

        //Assign control strings to global variables.
        defenseButtons = controlList[0];
        attackerButtons = controlList[0];
        //If there are 2 players on same team, assign second control to attacker's line.
        if (controlsCount == 2) attackerButtons = controlList[1];

        //Set the number of player on this team Side
        numberOfPlayers = controlsCount;
    }

    private void AssignHoldButtonToLinesTwoPlayers(TeamSide side)
    {
        //Left team
        if(side == TeamSide.LeftTeam)
        {
            ////Defender
            lines[0].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[0].leftButton;//Goalkeaper
            lines[1].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[0].rightButton;//Defense

            ////Attacker
            lines[2].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[1].leftButton;//Mid
            lines[3].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[1].rightButton;//Strikers
        }

        //Right team
        if (side == TeamSide.RightTeam)
        {
            ////Defender
            lines[0].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[0].rightButton;//Gk
            lines[1].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[0].leftButton;//Defense

            ////Attacker
            lines[2].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[1].rightButton;//Mid
            lines[3].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[1].leftButton;//Strikers
        }
    }

    private void AssignHoldButtonToLinesSinglePlyers(bool ballInTheMiddle, TeamSide side)
    {
        //Left team
        if (side == TeamSide.LeftTeam)
        {
            if (ballInTheMiddle)
            {
                lines[1].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[0].leftButton;
                lines[2].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[0].rightButton;
            }
            else
            {
                lines[1].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[0].rightButton;//defense
                lines[2].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.leftControllers[0].leftButton;//mid
            }
        }

        //Right team
        if (side == TeamSide.RightTeam)
        {
            if (ballInTheMiddle)
            {
                lines[1].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.rightControllers[0].rightButton;
                lines[2].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.rightControllers[0].leftButton;
            }
            else
            {
                lines[1].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.rightControllers[0].leftButton;//defense
                lines[2].GetComponent<LineMovement>().holdLineButton = MatchInfo._matchInfo.rightControllers[0].rightButton;//mid
            }
        }
    }

    private void ChangeLineIndicator(bool[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i])
                linesIndicators[i].sprite = activeLineSprite;
        }
    }
}
