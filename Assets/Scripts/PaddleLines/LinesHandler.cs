using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinesHandler : MonoBehaviour {

    //Left or Right player
    public enum PlayerSide
    {
        LeftPlayer,
        RightPlayer
    };

    public PlayerSide playerSide;
    private readonly float[] linesActiveBallLimit = new float[3];

    //Reference to lines
    GameObject[] lines = new GameObject[4];
    //Reference to the active ball
    public GameObject ball;

    private void Start()
    {
        //Get the active ball
        ball = GameObject.FindGameObjectWithTag("Ball");
        //Get reference to each line
        for (int i = 0; i < transform.childCount; i++) lines[i] = transform.GetChild(i).gameObject;

        if (playerSide == PlayerSide.LeftPlayer) SetLinesActiveLimits(-8f, -3f, 3f);
        if (playerSide == PlayerSide.RightPlayer) SetLinesActiveLimits(8f, 3f, -3f);

    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        //If there's a ball in the field, get the two nearest behind ball
        if (ball != null)
        {
            if (playerSide == PlayerSide.LeftPlayer) GetClosetsLinesLeftSide();
            if (playerSide == PlayerSide.RightPlayer) GetClosetsLinesRightSide();
        }//If not get the reference to the ball
        else ball = GameObject.FindGameObjectWithTag("Ball");
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
}
