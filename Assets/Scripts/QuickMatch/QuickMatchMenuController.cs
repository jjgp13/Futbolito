using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuickMatchMenuController : MonoBehaviour {

    public static QuickMatchMenuController controller;

    public Animator anim;
    public List<int> controlNumbersForLeftTeam = new List<int>();
    public List<int> controlNumbersForRightTeam = new List<int>();
    public List<ControlMapping> leftControls = new List<ControlMapping>();
    public List<ControlMapping> rightControls = new List<ControlMapping>();

    /// <summary>
    /// To check which panel is active
    /// </summary>
    public bool controllersPanel;
    public bool selectionTeamPanel;
    public bool settingsPanel;
    //0 defender and 1 attacker

    private void Awake()
    {
        if (controller == null) controller = this;
        else if (controller != this) Destroy(gameObject);
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
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
                //Debug.Log("Controls Created");
                SetControlls(controlNumbersForLeftTeam, leftControls);
                SetControlls(controlNumbersForRightTeam, rightControls);
                //Change to next panel
                controllersPanel = false;
                selectionTeamPanel = true;
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
            string left = "Left_Button_P" + (controlsNumber[i] + 1).ToString();
            string right = "Right_Button_P" + (controlsNumber[i] + 1).ToString();
            ControlMapping control = new ControlMapping(xAxis, yAxis, shoot, attractBall, left, right);
            teamSide.Add(control);
        }
        
    }

    public void ChangeToNextPanel()
    {

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
