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

    [Header("Panel transition animator")]
    public Animator anim;

    [Header("Contrllers assigned to each team")]
    //0 defender and 1 attacker
    public List<int> controlNumbersForLeftTeam = new List<int>();
    public List<int> controlNumbersForRightTeam = new List<int>();
    public List<ControlMapping> leftControls = new List<ControlMapping>();
    public List<ControlMapping> rightControls = new List<ControlMapping>();

    [Header("Check which panel is showed in scene")]
    public bool controllersPanel;
    public bool selectionTeamPanel;
    public bool settingsPanel;

    [Header("First elements selected")]
    public Button firstTeam;
    public Button uniform;

    private void Awake()
    {
        if (controller == null) controller = this;
        else if (controller != this) Destroy(gameObject);
        anim = GetComponent<Animator>();

        controllersPanel = true;
        selectionTeamPanel = false;
        settingsPanel = false;
        
    }
    

    private void Update()
    {
        //If anyone press start, set controls 
        if (Input.GetButtonDown("Start_Button") && controllersPanel)
        {
            //Clear list of controls, if player wants to change configuration
            leftControls.Clear();
            rightControls.Clear();
            
            if(controlNumbersForLeftTeam.Count > 0 || controlNumbersForRightTeam.Count > 0)
            {
                SetControlls(controlNumbersForLeftTeam, leftControls);
                SetControlls(controlNumbersForRightTeam, rightControls);
                //Change to next panel
                controllersPanel = false;
                selectionTeamPanel = true;
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

        //If anyone press start, set controls 
        if (Input.GetButtonDown("Start_Button") && selectionTeamPanel)
        {
            if (MatchInfo._matchInfo.leftTeam != null && MatchInfo._matchInfo.rightTeam != null)
            {
                SetControlls(controlNumbersForLeftTeam, leftControls);
                SetControlls(controlNumbersForRightTeam, rightControls);
                //Change to next panel
                selectionTeamPanel = false;
                settingsPanel = true;
                anim.SetTrigger("TeamOptions");
            }
            else
            {
                //Change to Team selection panel
                Debug.Log("Select teams");
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
            string left = "Left_Button_P" + (controlsNumber[i] + 1).ToString();
            string right = "Right_Button_P" + (controlsNumber[i] + 1).ToString();
            ControlMapping control = new ControlMapping(xAxis, yAxis, shoot, attractBall, left, right);
            teamSide.Add(control);
        }
        
    }

    public void AssignControlsToMachInfoObject()
    {
        if(leftControls.Count > 0)
        {
            MatchInfo._matchInfo.leftControlsAssigned = leftControls.Count;
            MatchInfo._matchInfo.leftControllers = leftControls;

            inputModule.horizontalAxis = leftControls[0].xAxis;
            inputModule.verticalAxis = leftControls[0].yAxis;
            inputModule.submitButton = leftControls[0].shootButton;
            inputModule.cancelButton = leftControls[0].attractButton;
        }
        else 
        {
            inputModule.horizontalAxis = rightControls[0].xAxis;
            inputModule.verticalAxis = rightControls[0].yAxis;
            inputModule.submitButton = rightControls[0].shootButton;
            inputModule.cancelButton = rightControls[0].attractButton;
            MatchInfo._matchInfo.rightControlsAssigned = rightControls.Count;
            MatchInfo._matchInfo.rightControllers = rightControls;
        }
    }

    public void SwitchUIControls()
    {
        if(rightControls.Count > 0 && leftControls.Count > 0)
        {
            if(inputModule.submitButton == leftControls[0].shootButton)
            {
                inputModule.horizontalAxis = rightControls[0].xAxis;
                inputModule.verticalAxis = rightControls[0].yAxis;
                inputModule.submitButton = rightControls[0].shootButton;
                inputModule.cancelButton = rightControls[0].attractButton;
            }
            else
            {
                inputModule.horizontalAxis = leftControls[0].xAxis;
                inputModule.verticalAxis = leftControls[0].yAxis;
                inputModule.submitButton = leftControls[0].shootButton;
                inputModule.cancelButton = leftControls[0].attractButton;
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
