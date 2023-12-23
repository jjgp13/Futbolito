using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLinesHandler : MonoBehaviour
{
    [Header("Team side")]
    //Left or Right player
    public TeamSide teamSide;
    private int lineIndex;

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
        //Get the active ball
        ball = GameObject.FindGameObjectWithTag("Ball");
        //When game start, check the number of controlls in each team
        //If the team is not handle by AI, check type of controls select
        if (teamSide == TeamSide.LeftTeam) SetLinesActiveLimits(-8.3f, -3f, 3f);
        if (teamSide == TeamSide.RightTeam) SetLinesActiveLimits(8.3f, 3f, -3f);
    }

    private void FixedUpdate()
    {
        //If there's a ball in the field, get the two nearest behind ball
        if (ball != null)
        {
            if (teamSide == TeamSide.LeftTeam) GetClosetsLinesLeftSide();
            if (teamSide == TeamSide.RightTeam) GetClosetsLinesRightSide();
        }//If not get the reference to the ball
        else ball = GameObject.FindGameObjectWithTag("Ball");
    }

    /// <summary>
    /// Given ball x position in field, active lines
    /// </summary>
    private void GetClosetsLinesLeftSide()
    {
        float ballPos = ball.transform.position.x;
        bool[] linesConfiguation;

        if (ballPos < linesActiveBallLimit[0])
            linesConfiguation = new bool[] { true, false, false, false };
        else if (ballPos < linesActiveBallLimit[1])
            linesConfiguation = new bool[] { true, true, false, false };
        else if (ballPos > linesActiveBallLimit[2])
            linesConfiguation = new bool[] { false, false, true, true };
        else
            linesConfiguation = new bool[] { false, true, true, false };

        ActivateLines(linesConfiguation);
        ChangeLineIndicator(linesConfiguation);
    }

    private void GetClosetsLinesRightSide()
    {
        float ballPos = ball.transform.position.x;
        bool[] linesConfiguation;

        if (ballPos > linesActiveBallLimit[0])
            linesConfiguation = new bool[] { true, false, false, false };
        else if (ballPos > linesActiveBallLimit[1])
            linesConfiguation = new bool[] { true, true, false, false };
        else if (ballPos < linesActiveBallLimit[2])
            linesConfiguation = new bool[] { false, false, true, true };
        else
            linesConfiguation = new bool[] { false, true, true, false };

        ActivateLines(linesConfiguation);
        ChangeLineIndicator(linesConfiguation);
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
    /// Select a line to activate
    /// </summary>
    /// <param name="conf">Bool array with lines to active 0 is goalkeeper - 3 are strikers</param>
    void ActivateLines(bool[] conf)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].GetComponent<LineMovement>() != null)
                lines[i].GetComponent<LineMovement>().isActive = conf[i];
            if (lines[i].GetComponent<NPCLineMovement>() != null)
                lines[i].GetComponent<NPCLineMovement>().isActive = conf[i];

            for (int j = 0; j < lines[i].transform.childCount; j++)
                lines[i].transform.GetChild(j).GetComponent<Animator>().SetBool("Active", conf[i]);
        }
        for (int i = 0; i < linesIndicators.Length; i++)
        {
            if (conf[i]) linesIndicators[i].sprite = activeLineSprite;
            else linesIndicators[i].sprite = inactiveLineSprite;
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
