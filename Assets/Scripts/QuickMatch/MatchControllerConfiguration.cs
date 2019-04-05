using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MatchControllerConfiguration : MonoBehaviour
{
    public Sprite comCircleImage;
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
    /// <summary>
    /// Check if players are active [0]: player 1, [1]: player 2 and so on
    /// Same for position
    /// Player could be at Middle or left team or right team;
    /// </summary>
    private bool[] isPlayerActive = new bool[4] { false, false, false, false};
    private string[] playerPositon = new string[4] { "Middle", "Middle", "Middle", "Middle" };
    
    public List<int> leftTeamControllers = new List<int>();
    public List<int> rightTeamControllers = new List<int>();
    

    // Update is called once per frame
    void Update()
    {
        //Check to activate
        if(Input.GetButtonDown("Shoot_Button_P1")) 
            CheckInputToActivatePlayers(isPlayerActive[0], p1Ball, 0);
        if (Input.GetButtonDown("Shoot_Button_P2")) 
            CheckInputToActivatePlayers(isPlayerActive[1], p2Ball, 1);
        if (Input.GetButtonDown("Shoot_Button_P3")) 
            CheckInputToActivatePlayers(isPlayerActive[3], p3Ball, 2);
        if (Input.GetButtonDown("Shoot_Button_P4")) 
            CheckInputToActivatePlayers(isPlayerActive[4], p4Ball, 3);

        //Check to change position
        if (Input.GetButtonDown("Left_Button_P1")) CheckInputToChooseTeamSide(isPlayerActive[0], "Left", playerPositon[0], p1Ball, 0);
        if (Input.GetButtonDown("Right_Button_P1")) CheckInputToChooseTeamSide(isPlayerActive[0], "Right", playerPositon[0], p1Ball, 0);

        if (Input.GetButtonDown("Left_Button_P2")) CheckInputToChooseTeamSide(isPlayerActive[1], "Left", playerPositon[1], p2Ball, 1);
        if (Input.GetButtonDown("Right_Button_P2")) CheckInputToChooseTeamSide(isPlayerActive[1], "Right", playerPositon[1], p2Ball, 1);

        if (Input.GetButtonDown("Left_Button_P3")) CheckInputToChooseTeamSide(isPlayerActive[2], "Left", playerPositon[2], p3Ball, 2);
        if (Input.GetButtonDown("Right_Button_P3")) CheckInputToChooseTeamSide(isPlayerActive[2], "Right", playerPositon[2], p3Ball, 2);

        if (Input.GetButtonDown("Left_Button_P4")) CheckInputToChooseTeamSide(isPlayerActive[3], "Left", playerPositon[3], p4Ball, 3);
        if (Input.GetButtonDown("Right_Button_P4")) CheckInputToChooseTeamSide(isPlayerActive[3], "Right", playerPositon[3], p4Ball, 3);
        SetControlsInMatchInfo();
        SetMatchType();
    }
    /// <summary>
    /// When player press Down Button, his ball is activate and now it can select a team
    /// </summary>
    /// <param name="isObjectActive">Check if it is active</param>
    /// <param name="playerBall">Image in the middle that represents player color</param>
    /// <param name="playerNumber">Number of controller 0 to 3 (Four players)</param>
    private void CheckInputToActivatePlayers(bool isObjectActive, Image playerBall, int playerNumber)
    {
        if (!isObjectActive)
        {
            playerBall.color = new Color(1f, 1f, 1f, 1f);
            isPlayerActive[playerNumber] = true;
        }
        
    }

    /// <summary>
    /// When player press left or right button, it can change to left or right team.
    /// </summary>
    /// <param name="isPlayerActive">Check if players is active</param>
    /// <param name="buttonPressed">Which button was pressed (Left or Right)</param>
    /// <param name="playerSpot">Where is the player in that moment</param>
    /// <param name="playerColor">Image that represents the player color</param>
    /// <param name="playerNumber">Controller's number</param>
    private void CheckInputToChooseTeamSide(bool isPlayerActive, string buttonPressed, string playerSpot, Image playerColor, int playerNumber)
    {
        if (isPlayerActive)
        {
            switch (playerSpot)
            {
                //Player is in the middle 
                case "Middle":
                    //Wants to play on left team
                    if (buttonPressed == "Left")
                    {
                        if(leftTeamControllers.Count == 0){
                            defenderleftTeamImages.sprite = playerColor.sprite;
                            attackerleftTeamImages.sprite = playerColor.sprite;
                            leftTeamControllers.Add(playerNumber);
                            leftTeamControllers.Add(playerNumber);
                        } else if(leftTeamControllers.Count == 2){
                            attackerleftTeamImages.sprite = playerColor.sprite;
                            leftTeamControllers.RemoveAt(1);
                            leftTeamControllers.Add(playerNumber);
                        }
                    }
                    //Wants to play right team
                    if (buttonPressed == "Right")
                    {
                        if (rightTeamControllers.Count == 0){
                            defenderRightTeamImages.sprite = playerColor.sprite;
                            attackerRightTeamImages.sprite = playerColor.sprite;
                            rightTeamControllers.Add(playerNumber);
                            rightTeamControllers.Add(playerNumber);
                        }else if (rightTeamControllers.Count == 2){
                            attackerRightTeamImages.sprite = playerColor.sprite;
                            rightTeamControllers.RemoveAt(1);
                            rightTeamControllers.Add(playerNumber);
                        }
                    }
                    playerColor.color = new Color(1f, 1f, 1f, 0.5f);
                    playerPositon[playerNumber] = buttonPressed;
                    break;
                case "Left":    //Player is in left team, only can go to middle
                    if (buttonPressed == "Right")
                    {
                        if (SamePlayer(leftTeamControllers))
                        {
                            leftTeamControllers.RemoveAll(i => i == playerNumber);
                            defenderleftTeamImages.sprite = comCircleImage;
                            attackerleftTeamImages.sprite = comCircleImage;
                        }
                        else
                        {
                            //Defender wants go to midle
                            if(playerNumber == leftTeamControllers[0])
                            {
                                leftTeamControllers[0] = leftTeamControllers[1];
                                defenderleftTeamImages.sprite = attackerleftTeamImages.sprite;
                            }//Attacker wants go to middle
                            else
                            {
                                leftTeamControllers[1] = leftTeamControllers[0];
                                attackerleftTeamImages.sprite = defenderleftTeamImages.sprite;
                            }
                        }
                        playerColor.color = new Color(1f, 1f, 1f, 1f);
                        playerPositon[playerNumber] = "Middle";
                    }
                    break;
                case "Right":   //Player is in right team, only can go to middle
                    if (buttonPressed == "Left"){
                        if (SamePlayer(rightTeamControllers)){
                            rightTeamControllers.RemoveAll(i => i == playerNumber);
                            defenderRightTeamImages.sprite = comCircleImage;
                            attackerRightTeamImages.sprite = comCircleImage;
                            playerColor.color = new Color(1f, 1f, 1f, 1f);
                            playerPositon[playerNumber] = "Middle";
                        }
                        else
                        {
                            //Defender wants go to midle
                            if (playerNumber == rightTeamControllers[0])
                            {
                                rightTeamControllers[0] = rightTeamControllers[1];
                                defenderRightTeamImages.sprite = attackerRightTeamImages.sprite;
                            }//Attacker wants go to middle
                            else
                            {
                                rightTeamControllers[1] = rightTeamControllers[0];
                                attackerRightTeamImages.sprite = defenderRightTeamImages.sprite;
                            }
                        }
                        playerColor.color = new Color(1f, 1f, 1f, 1f);
                        playerPositon[playerNumber] = "Middle";
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Set match text given player numbers assigned to each team
    /// </summary>
    private void SetMatchType()
    {
        if(leftTeamControllers.Count == 0 && rightTeamControllers.Count == 0)
            matchType.text = "COM VS COM";

        if (leftTeamControllers.Count == 2 && rightTeamControllers.Count == 0)
            if(SamePlayer(leftTeamControllers))
                matchType.text = "1P VS COM";
            else
                matchType.text = "2P VS COM";

        if (leftTeamControllers.Count == 0 && rightTeamControllers.Count == 2)
            if (SamePlayer(rightTeamControllers))
                matchType.text = "COM VS 1P";
            else
                matchType.text = "COM VS 2P";

        if (leftTeamControllers.Count == 2 && rightTeamControllers.Count == 2)
            if (SamePlayer(leftTeamControllers) && SamePlayer(rightTeamControllers))
                matchType.text = "1P VS 1P";
            else
                matchType.text = "2P VS 2P";
    }

    /// <summary>
    /// Check if a team has same player assigned for both defender and attacker
    /// </summary>
    /// <param name="q">Queue with players numbers</param>
    /// <returns>true if is the same player for both, false if there are two players</returns>
    bool SamePlayer(List<int> q)
    {
        int[] arr = q.ToArray();
        if (arr[0] == arr[1]) return true;
        return false;
    }

    /// <summary>
    /// Set controller in match info
    /// Match info will carry the info to match scene to map controls in paddles lines
    /// </summary>
    private void SetControlsInMatchInfo()
    {
        MatchInfo._matchInfo.defenderLeftController = leftTeamControllers[0];
        MatchInfo._matchInfo.attackerLeftController = leftTeamControllers[1];
        MatchInfo._matchInfo.defenderRightController = rightTeamControllers[0];
        MatchInfo._matchInfo.attackerRightController = rightTeamControllers[1];
    }
}