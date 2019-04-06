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


    // Update is called once per frame
    void Update()
    {
        if (QuickMatchMenuController.controller.controllersPanel)
        {
            //Check to activate
            if (Input.GetButtonDown("Shoot_Button_P1"))
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
            //SetControlsInMatchInfo();
            SetMatchType();
        }
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
                        if(QuickMatchMenuController.controller.controlNumbersForLeftTeam.Count == 0){
                            defenderleftTeamImages.sprite = playerColor.sprite;
                            attackerleftTeamImages.sprite = playerColor.sprite;
                            QuickMatchMenuController.controller.controlNumbersForLeftTeam.Add(playerNumber);
                            QuickMatchMenuController.controller.controlNumbersForLeftTeam.Add(playerNumber);
                        } else if(QuickMatchMenuController.controller.controlNumbersForLeftTeam.Count == 2){
                            attackerleftTeamImages.sprite = playerColor.sprite;
                            QuickMatchMenuController.controller.controlNumbersForLeftTeam.RemoveAt(1);
                            QuickMatchMenuController.controller.controlNumbersForLeftTeam.Add(playerNumber);
                        }
                    }
                    //Wants to play right team
                    if (buttonPressed == "Right")
                    {
                        if (QuickMatchMenuController.controller.controlNumbersForRightTeam.Count == 0){
                            defenderRightTeamImages.sprite = playerColor.sprite;
                            attackerRightTeamImages.sprite = playerColor.sprite;
                            QuickMatchMenuController.controller.controlNumbersForRightTeam.Add(playerNumber);
                            QuickMatchMenuController.controller.controlNumbersForRightTeam.Add(playerNumber);
                        }else if (QuickMatchMenuController.controller.controlNumbersForRightTeam.Count == 2){
                            attackerRightTeamImages.sprite = playerColor.sprite;
                            QuickMatchMenuController.controller.controlNumbersForRightTeam.RemoveAt(1);
                            QuickMatchMenuController.controller.controlNumbersForRightTeam.Add(playerNumber);
                        }
                    }
                    playerColor.color = new Color(1f, 1f, 1f, 0.5f);
                    playerPositon[playerNumber] = buttonPressed;
                    break;
                case "Left":    //Player is in left team, only can go to middle
                    if (buttonPressed == "Right")
                    {
                        if (SamePlayer(QuickMatchMenuController.controller.controlNumbersForLeftTeam))
                        {
                            QuickMatchMenuController.controller.controlNumbersForLeftTeam.RemoveAll(i => i == playerNumber);
                            defenderleftTeamImages.sprite = comCircleImage;
                            attackerleftTeamImages.sprite = comCircleImage;
                        }
                        else
                        {
                            //Defender wants go to midle
                            if(playerNumber == QuickMatchMenuController.controller.controlNumbersForLeftTeam[0])
                            {
                                QuickMatchMenuController.controller.controlNumbersForLeftTeam[0] = QuickMatchMenuController.controller.controlNumbersForLeftTeam[1];
                                defenderleftTeamImages.sprite = attackerleftTeamImages.sprite;
                            }//Attacker wants go to middle
                            else
                            {
                                QuickMatchMenuController.controller.controlNumbersForLeftTeam[1] = QuickMatchMenuController.controller.controlNumbersForLeftTeam[0];
                                attackerleftTeamImages.sprite = defenderleftTeamImages.sprite;
                            }
                        }
                        playerColor.color = new Color(1f, 1f, 1f, 1f);
                        playerPositon[playerNumber] = "Middle";
                    }
                    break;
                case "Right":   //Player is in right team, only can go to middle
                    if (buttonPressed == "Left"){
                        if (SamePlayer(QuickMatchMenuController.controller.controlNumbersForRightTeam)){
                            QuickMatchMenuController.controller.controlNumbersForRightTeam.RemoveAll(i => i == playerNumber);
                            defenderRightTeamImages.sprite = comCircleImage;
                            attackerRightTeamImages.sprite = comCircleImage;
                        }
                        else
                        {
                            //Defender wants go to midle
                            if (playerNumber == QuickMatchMenuController.controller.controlNumbersForRightTeam[0])
                            {
                                QuickMatchMenuController.controller.controlNumbersForRightTeam[0] = QuickMatchMenuController.controller.controlNumbersForRightTeam[1];
                                defenderRightTeamImages.sprite = attackerRightTeamImages.sprite;
                            }//Attacker wants go to middle
                            else
                            {
                                QuickMatchMenuController.controller.controlNumbersForRightTeam[1] = QuickMatchMenuController.controller.controlNumbersForRightTeam[0];
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
        if(QuickMatchMenuController.controller.controlNumbersForLeftTeam.Count == 0 && QuickMatchMenuController.controller.controlNumbersForRightTeam.Count == 0)
            matchType.text = "COM VS COM";

        if (QuickMatchMenuController.controller.controlNumbersForLeftTeam.Count == 2 && QuickMatchMenuController.controller.controlNumbersForRightTeam.Count == 0)
            if(SamePlayer(QuickMatchMenuController.controller.controlNumbersForLeftTeam))
                matchType.text = "1P VS COM";
            else
                matchType.text = "2P VS COM";

        if (QuickMatchMenuController.controller.controlNumbersForLeftTeam.Count == 0 && QuickMatchMenuController.controller.controlNumbersForRightTeam.Count == 2)
            if (SamePlayer(QuickMatchMenuController.controller.controlNumbersForRightTeam))
                matchType.text = "COM VS 1P";
            else
                matchType.text = "COM VS 2P";

        if (QuickMatchMenuController.controller.controlNumbersForLeftTeam.Count == 2 && QuickMatchMenuController.controller.controlNumbersForRightTeam.Count == 2)
            if (SamePlayer(QuickMatchMenuController.controller.controlNumbersForLeftTeam) && SamePlayer(QuickMatchMenuController.controller.controlNumbersForRightTeam))
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
        MatchInfo._matchInfo.defenderLeftController = QuickMatchMenuController.controller.controlNumbersForLeftTeam[0];
        MatchInfo._matchInfo.attackerLeftController = QuickMatchMenuController.controller.controlNumbersForLeftTeam[1];
        MatchInfo._matchInfo.defenderRightController = QuickMatchMenuController.controller.controlNumbersForRightTeam[0];
        MatchInfo._matchInfo.attackerRightController = QuickMatchMenuController.controller.controlNumbersForRightTeam[1];
    }
}