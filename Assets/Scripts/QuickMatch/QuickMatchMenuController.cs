using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class QuickMatchMenuController : MonoBehaviour {

    public static QuickMatchMenuController controller;
    
    [Header("Event System Reference")]
    public GameObject eventSystem;
    public StandaloneInputModule inputModule;
    public string leftButtonString;
    public string rightButtonString;

    [Header("Panel transition animator")]
    public Animator anim;

    [Header("Contrllers assigned to each team")]
    //0 defender and 1 attacker
    public List<int> controlNumbersForLeftTeam = new List<int>();
    public List<int> controlNumbersForRightTeam = new List<int>();
    public List<ControlMapping> leftControls = new List<ControlMapping>();
    public List<ControlMapping> rightControls = new List<ControlMapping>();

    [Header("Check which panel is showed in scene")]
    public bool isControllerPanelActive;
    public bool isSelectionTeamPanelActive;
    public bool isSettingsPanelActive;

    [Header("First elements selected")]
    public Button firstTeam;
    public Button uniform;

    private void Awake()
    {
        if (controller == null) controller = this;
        else if (controller != this) Destroy(gameObject);
        anim = GetComponent<Animator>();

        isControllerPanelActive = true;
        isSelectionTeamPanelActive = false;
        isSettingsPanelActive = false;
        
    }
    

    private void Update()
    {
        //If anyone press start, on selection teams panel
        if (Input.GetButtonDown("Start_Button") && isSelectionTeamPanelActive)
        {
            //Check if teams have been assigned, if not send a message.
            if (MatchInfo._matchInfo.leftTeam != null && MatchInfo._matchInfo.rightTeam != null)
            {
                //Change to next panel
                isSelectionTeamPanelActive = false;
                isSettingsPanelActive = true;
                anim.SetTrigger("TeamOptions");
                uniform.Select();
            }
            else
            {
                //Change to Team selection panel
                Debug.Log("Select teams");
            }
        }

        //If anyone press start, on controlls panel
        if (Input.GetButtonDown("Start_Button") && isControllerPanelActive)
        {
            //Clear list of controls, if player wants to change configuration
            leftControls.Clear();
            rightControls.Clear();
            
            if(controlNumbersForLeftTeam.Count > 0 || controlNumbersForRightTeam.Count > 0)
            {
                SetControlls(controlNumbersForLeftTeam, leftControls);
                SetControlls(controlNumbersForRightTeam, rightControls);
                //Change to next panel
                isControllerPanelActive = false;
                isSelectionTeamPanelActive = true;
                anim.SetTrigger("TeamSelection");
                //Assign controls to match info
                AssignControlsToMachInfoObject();

                //Activate event system and set controls first for left team
                eventSystem.SetActive(true);
            }
            else
            {
                //Change to Team selection panel
                Debug.Log("No controls assigned");
            }
        }

        
    }

    /// <summary>
    /// Map string to handle control input
    /// </summary>
    /// <param name="controlsNumber">List of integers with numbers of player (first elemetn controls defense, second attack)</param>
    /// <param name="teamSide">For which team side is going to map these numbers</param>
    public void SetControlls(List<int> controlsNumber, List<ControlMapping> teamSide)
    {
        for (int i = 0; i < controlsNumber.Count; i++)
        {
            string xAxis = "Left_Joystick_Horizontal_P" + (controlsNumber[i] + 1).ToString();
            string yAxis = "Left_Joystick_Vertical_P" + (controlsNumber[i] + 1).ToString();
            string shoot = "Shoot_Button_P" + (controlsNumber[i] + 1).ToString();
            string attractBall = "Attract_Ball_Button_P" + (controlsNumber[i] + 1).ToString();
            string wallPass = "Wall_Pass_Button_P" + (controlsNumber[i] + 1).ToString();
            string left = "Left_Button_P" + (controlsNumber[i] + 1).ToString();
            string right = "Right_Button_P" + (controlsNumber[i] + 1).ToString();
            ControlMapping control = new ControlMapping(xAxis, yAxis, shoot, attractBall, wallPass,left, right);
            teamSide.Add(control);
        }
    }

    /// <summary>
    /// Match info object will handle which controlls are assigned to each team
    /// And to each line (if there are two players for each team)
    /// </summary>
    public void AssignControlsToMachInfoObject()
    {
        //If left controls are assigned to one player. Give the UI control to left team player
        if(leftControls.Count > 0)
        {
            MatchInfo._matchInfo.leftControlsAssigned = controlNumbersForLeftTeam;
            MatchInfo._matchInfo.leftControllers = leftControls;

            inputModule.horizontalAxis = leftControls[0].xAxis;
            inputModule.verticalAxis = leftControls[0].yAxis;
            inputModule.submitButton = leftControls[0].shootButton;
            inputModule.cancelButton = leftControls[0].attractButton;

            leftButtonString = leftControls[0].leftButton;
            rightButtonString = leftControls[0].rightButton;
        }
        else //otherwise give UI control to right team player.
        {
            MatchInfo._matchInfo.rightControlsAssigned = controlNumbersForRightTeam;
            MatchInfo._matchInfo.rightControllers = rightControls;

            inputModule.horizontalAxis = rightControls[0].xAxis;
            inputModule.verticalAxis = rightControls[0].yAxis;
            inputModule.submitButton = rightControls[0].shootButton;
            inputModule.cancelButton = rightControls[0].attractButton;
            leftButtonString = rightControls[0].leftButton;
            rightButtonString = rightControls[0].rightButton;
        }
    }

    /// <summary>
    /// Change the control of UI to the other player.
    /// If there is just one player agains computer, it should not change the ui controls
    /// </summary>
    public void SwitchUIControls()
    {
        //If there are a player for each side, control ui should be switched.
        if(rightControls.Count > 0 && leftControls.Count > 0)
        {
            if(inputModule.submitButton == leftControls[0].shootButton)
            {
                inputModule.horizontalAxis = rightControls[0].xAxis;
                inputModule.verticalAxis = rightControls[0].yAxis;
                inputModule.submitButton = rightControls[0].shootButton;
                inputModule.cancelButton = rightControls[0].attractButton;
                leftButtonString = rightControls[0].leftButton;
                rightButtonString = rightControls[0].rightButton;
            }
            if (inputModule.submitButton == rightControls[0].shootButton)
            {
                inputModule.horizontalAxis = leftControls[0].xAxis;
                inputModule.verticalAxis = leftControls[0].yAxis;
                inputModule.submitButton = leftControls[0].shootButton;
                inputModule.cancelButton = leftControls[0].attractButton;
                leftButtonString = leftControls[0].leftButton;
                rightButtonString = leftControls[0].rightButton;
            }
        }
    }

    public void ChangeToNextPanel(string panel)
    {
        if (panel == "SelectionPanel") firstTeam.Select();
        if (panel == "OptionsPanel") uniform.Select();
    }


    public void ChangeScene(string sceneName)
    {
        if (sceneName == "MainMenu")
        {
            Destroy(GameObject.Find("MatchInfo"));
            Destroy(GameObject.FindGameObjectWithTag("PlayerDataObject"));
        }
        SceneManager.LoadScene(sceneName);
    }
}
