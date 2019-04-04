using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MatchControllerConfiguration : MonoBehaviour
{

    [Header("References to UI")]
    public Image p1Ball;
    public Image p2Ball;
    public Image p3Ball;
    public Image p4Ball;
    public Image defenderleftTeamImages;
    public Image attackerleftTeamImages;
    public Image defenderRightTeamImages;
    public Image attackerRightTeamImages;
    public Text matchType;

    private bool isActiveP1;
    private bool isActiveP2;
    private bool isActiveP3;
    private bool isActiveP4;
    private string teamP1;
    private string teamP2;
    private string teamP3;
    private string teamP4;
    

    private Queue<int> leftTeamControllers = new Queue<int>();
    private Queue<int> rightTeamControllers = new Queue<int>();
    

    private void Awake()
    {
        isActiveP1 = false;
        isActiveP2 = false;
        isActiveP3 = false;
        isActiveP4 = false;
        teamP1 = "Centre";
        teamP2 = "Centre";
        teamP3 = "Centre";
        teamP4 = "Centre";
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Shoot_Button_P1")) 
            CheckInputToActivatePlayers(isActiveP1, p1Ball);
        if (Input.GetButtonDown("Shoot_Button_P2")) 
            CheckInputToActivatePlayers(isActiveP2, p2Ball);
        if (Input.GetButtonDown("Shoot_Button_P3")) 
            CheckInputToActivatePlayers(isActiveP3, p3Ball);
        if (Input.GetButtonDown("Shoot_Button_P4")) 
            CheckInputToActivatePlayers(isActiveP4, p4Ball);

    }

    private void CheckInputToActivatePlayers(bool isObjectActive, Image playerBall)
    {
        if (!isObjectActive)
        {
            isObjectActive = true;
            playerBall.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    private void CheckInputToChooseTeamSide(bool isObjectActive)
    {
        //P1 input
        if (isActiveP1)
        {
            if(teamP1 == "Centre")
            {
                if (Input.GetButtonDown("Left_Button_P1"))
                {
                    if(MatchInfo._matchInfo.leftControllers == 0)
                    {

                    }
                }
                if (Input.GetButtonDown("Right_Button_P1"))
                {
                    
                }
            }
            if(teamP1 == "Left")
            {

            }
            if (teamP1 == "Right")
            {

            }
        }
        
        
    }

    public void TeamSelected()
    {
        SetMatchType();
    }
    
    private void SetMatchType()
    {
        if(MatchInfo._matchInfo.leftControllers == 0 && MatchInfo._matchInfo.rightControllers == 0)
            matchType.text = "COM VS COM";
        if (MatchInfo._matchInfo.leftControllers == 1 && MatchInfo._matchInfo.rightControllers == 0)
            matchType.text = "1P VS COM";
        if (MatchInfo._matchInfo.leftControllers == 0 && MatchInfo._matchInfo.rightControllers == 1)
            matchType.text = "COM VS 1P";
        if (MatchInfo._matchInfo.leftControllers == 1 && MatchInfo._matchInfo.rightControllers == 1)
            matchType.text = "1P VS 1P";
        if (MatchInfo._matchInfo.leftControllers == 2 && MatchInfo._matchInfo.rightControllers == 1)
            matchType.text = "2P VS 1P";
        if (MatchInfo._matchInfo.leftControllers == 1 && MatchInfo._matchInfo.rightControllers == 2)
            matchType.text = "1P VS 2P";
        if (MatchInfo._matchInfo.leftControllers == 2 && MatchInfo._matchInfo.rightControllers == 2)
            matchType.text = "2P VS 2P";
    }
}