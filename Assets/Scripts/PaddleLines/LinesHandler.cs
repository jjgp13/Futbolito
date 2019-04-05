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

    [Header("Player info given numbers of players, player side and control type")]
    [Tooltip("Number of player that handles this team")]
    [Range(1,2)]
    public int numberOfPlayers = 1;
    //Default controller to map strings for buttons
    public PlayerNumber defenderPlayer;
    public string changeLineLeftButtonDefender;
    public string changeLineRightButtonDefender;
    public string moveLineDefender;
    public string shootButtonDefender;
    public string attractButtonDefender;
    //Second controller for this team
    public PlayerNumber attackerPlayer;
    public string changeLineLeftButtonAttacker;
    public string changeLineRightButtonAttacker;
    public string moveLineAttacker;
    public string shootButtonAttacker;
    public string attractButtonAttacker;



    //Type of Control
    /// <summary>
    /// If automatic is picked. Lines will active automatic given ball position.
    /// if manual is picked. Active lines change given player's input.
    /// </summary>
    public ControlType controlType;
    private int lineIndex;

    //Left or Right player
    public TeamSide playerSide;
    private readonly float[] linesActiveBallLimit = new float[3];

    //Reference to the active ball
    private GameObject ball;

    [Header("Reference to child gameobjects (lines and indicators)")]
    //Reference to lines
    public GameObject[] lines = new GameObject[4];

    //reference to indicators to see which line is active
    public SpriteRenderer[] linesIndicators = new SpriteRenderer[4];
    public Sprite inactiveLineSprite;
    public Sprite activeLineSprite;

    private void Start()
    {
        
        //Set buttons given players
        MapButtonsToPlayer();

        if(numberOfPlayers == 1)
        {
            if (controlType == ControlType.Automatic)
            {
                //Get the active ball
                ball = GameObject.FindGameObjectWithTag("Ball");
                //Get reference to each line

                if (playerSide == TeamSide.LeftTeam) SetLinesActiveLimits(-8f, -3f, 3f);
                if (playerSide == TeamSide.RightTeam) SetLinesActiveLimits(8f, 3f, -3f);
            }

            if (controlType == ControlType.Manual)
            {
                lineIndex = 1;
                ActivateLines(new bool[] { false, true, true, false });
            }
        }
        else
        {
            ActivateLines(new bool[] { true, true, true, true });
        }
        
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        if(numberOfPlayers == 1)
        {
            if (controlType == ControlType.Automatic)
            {
                //If there's a ball in the field, get the two nearest behind ball
                if (ball != null)
                {
                    if (playerSide == TeamSide.LeftTeam) GetClosetsLinesLeftSide();
                    if (playerSide == TeamSide.RightTeam) GetClosetsLinesRightSide();
                }//If not get the reference to the ball
                else ball = GameObject.FindGameObjectWithTag("Ball");
            }

            if (controlType == ControlType.Manual && numberOfPlayers == 1)
            {
                //Change lines to left
                if (Input.GetButtonDown(changeLineLeftButtonDefender))
                    if (lineIndex > 0) lineIndex--;
                //Change lines to right
                if (Input.GetButtonDown(changeLineRightButtonDefender))
                    if (lineIndex < 2) lineIndex++;
                //Active line given index
                ManualActiveLines(lineIndex);
            }
        }
        
    }

    /// <summary>
    /// Given ball x position in field, active lines
    /// </summary>
    private void GetClosetsLinesLeftSide()
    {
        float ballPos = ball.transform.position.x;
        if (ballPos < linesActiveBallLimit[0])
            ActivateLines(new bool[] {true, false, false, false});
        else if (ballPos < linesActiveBallLimit[1])
            ActivateLines(new bool[] { true, true, false, false });
        else if (ballPos > linesActiveBallLimit[2])
            ActivateLines(new bool[] { false, false, true, true});
        else
            ActivateLines(new bool[] { false, true, true, false });
    }

    private void GetClosetsLinesRightSide()
    {
        float ballPos = ball.transform.position.x;
        if (ballPos > linesActiveBallLimit[0])
            ActivateLines(new bool[] { true, false, false, false });
        else if (ballPos > linesActiveBallLimit[1])
            ActivateLines(new bool[] { true, true, false, false });
        else if (ballPos < linesActiveBallLimit[2])
            ActivateLines(new bool[] { false, false, true, true });
        else
            ActivateLines(new bool[] { false, true, true, false });
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
                lines[i].transform.GetChild(j).GetComponent<Animator>().SetBool("Active", conf[i]);
        }
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

    private void MapButtonsToPlayer()
    {
        if(numberOfPlayers == 1)
        {
            switch (defenderPlayer)
            {
                case PlayerNumber.Player1:
                    moveLineDefender = "Left_Joystick_Vertical_P1";
                    changeLineLeftButtonDefender = "Left_Button_P1";
                    changeLineRightButtonDefender = "Right_Button_P1";
                    shootButtonDefender = "Shoot_Button_P1";
                    attractButtonDefender = "Attract_Ball_Button_P1";
                    break;
                case PlayerNumber.Player2:
                    moveLineDefender = "Left_Joystick_Vertical_P2";
                    changeLineLeftButtonDefender = "Left_Button_P2";
                    changeLineRightButtonDefender = "Right_Button_P2";
                    shootButtonDefender = "Shoot_Button_P2";
                    attractButtonDefender = "Attract_Ball_Button_P2";
                    break;
                case PlayerNumber.Player3:
                    moveLineDefender = "Left_Joystick_Vertical_P3";
                    changeLineLeftButtonDefender = "Left_Button_P3";
                    changeLineRightButtonDefender = "Right_Button_P3";
                    shootButtonDefender = "Shoot_Button_P3";
                    attractButtonDefender = "Attract_Ball_Button_P3";
                    break;
                case PlayerNumber.Player4:
                    moveLineDefender = "Left_Joystick_Vertical_P4";
                    changeLineLeftButtonDefender = "Left_Button_P4";
                    changeLineRightButtonDefender = "Right_Button_P4";
                    shootButtonDefender = "Shoot_Button_P4";
                    attractButtonDefender = "Attract_Ball_Button_P4";
                    break;
                case PlayerNumber.COM:
                    break;
                default:
                    moveLineDefender = "Left_Joystick_Vertical_P1";
                    changeLineLeftButtonDefender = "Left_Button_P1";
                    changeLineRightButtonDefender = "Right_Button_P1";
                    shootButtonDefender = "Shoot_Button_P1";
                    attractButtonDefender = "Attract_Ball_Button_P1";
                    break;
            }
        }
        
    }
}
