using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionTeamsController : MonoBehaviour
{
    [Header("Teams flag button prefab")]
    public Button teamButton;
    
    [Header("Left Team UI")]
    public GameObject leftTeamsPanel;
    public Text leftRegionText;
    public GameObject leftTeamFlag;
    private int leftRegionIndex;

    [Header("Right Team UI")]
    public GameObject rightTeamsPanel;
    public Text rightRegionText;
    public GameObject rightTeamFlag;
    private int rightRegionIndex;

    [Header("Map image")]
    public Image mapImage;
    public Sprite[] mapSprites;

    [Header("Canvas groups (Left team and right team)")]
    public CanvasGroup leftCanvas;
    public CanvasGroup rightCanvas;

    //America: 0, Europe: 1, Africa:2, Asia:4
    private List<Team> americanTeams = new List<Team>();
    private List<Team> europeanTeams = new List<Team>();
    private List<Team> africanTeams = new List<Team>();
    private List<Team> asianTeams = new List<Team>();

    int begin, end;

    private void Awake()
    {
        //Left team starting showing american region teams
        leftRegionIndex = 0;
        //Right team starting showing asia region teams
        rightRegionIndex = 3;
        //Load all teams and fill each region team list
        FillTeamRegions();
    }

    private void Start()
    {
        //Fill left and right panels with button flags
        FillTeamsPanel(americanTeams, leftTeamsPanel, 0, 6);
        FillTeamsPanel(asianTeams, rightTeamsPanel, 0,6);
        //Select first button in left teams panel
        leftTeamsPanel.transform.GetChild(0).gameObject.GetComponent<Button>().Select();
    }

    private void Update()
    {
        if (QuickMatchMenuController.controller.selectionTeamPanel)
        {
            if(QuickMatchMenuController.controller.leftControls.Count > 0)
            {
                //////Left team controller input
                //Left button
                if (Input.GetButtonDown(QuickMatchMenuController.controller.leftControls[0].leftButton))
                {
                    if (leftRegionIndex == 0) leftRegionIndex = 3;
                    else leftRegionIndex--;
                    SelectedConf(leftRegionIndex, leftTeamsPanel, leftRegionText);
                    mapImage.sprite = ChangeMapSprite(leftRegionIndex, rightRegionIndex);
                }
                //Right button    
                if (Input.GetButtonDown(QuickMatchMenuController.controller.leftControls[0].rightButton))
                {
                    if (leftRegionIndex == 3) leftRegionIndex = 0;
                    else leftRegionIndex++;
                    SelectedConf(leftRegionIndex, leftTeamsPanel, leftRegionText);
                    mapImage.sprite = ChangeMapSprite(leftRegionIndex, rightRegionIndex);
                }
            }

            if (QuickMatchMenuController.controller.rightControls.Count > 0)
            {
                //////Right team controller input
                //Left button
                if (Input.GetButtonDown(QuickMatchMenuController.controller.rightControls[0].leftButton))
                {
                    if (rightRegionIndex == 0) rightRegionIndex = 3;
                    else rightRegionIndex--;
                    SelectedConf(rightRegionIndex, rightTeamsPanel, rightRegionText);
                    mapImage.sprite = ChangeMapSprite(leftRegionIndex, rightRegionIndex);
                }
                //Right button
                if (Input.GetButtonDown(QuickMatchMenuController.controller.rightControls[0].rightButton))
                {
                    if (rightRegionIndex == 3) rightRegionIndex = 0;
                    else rightRegionIndex++;
                    SelectedConf(rightRegionIndex, rightTeamsPanel, rightRegionText);
                    mapImage.sprite = ChangeMapSprite(leftRegionIndex, rightRegionIndex);
                }

            }

        }
        
    }
  


    /// <summary>
    /// This method will load all teams from resources folder and separete them by region.
    /// </summary>
    private void FillTeamRegions()
    {
        Team[] allTeams = Resources.LoadAll<Team>("Teams");
        foreach (Team team in allTeams)
        {
            switch (team.region)
            {
                case "America":
                    americanTeams.Add(team);
                    break;
                case "Africa":
                    africanTeams.Add(team);
                    break;
                case "Europe":
                    europeanTeams.Add(team);
                    break;
                case "Asia":
                    asianTeams.Add(team);
                    break;
            }
        }
    }

    /// <summary>
    /// Change panel teams when region is selected
    /// </summary>
    /// <param name="region">Region selected (0:America, 1:Europe, 2:Africa, 3:Asia)</param>
    /// <param name="teamsPanel">Left Team or Right Team</param>
    /// <param name="regionText">Reference to Text box above teams panel</param>
    public void SelectedConf(int region, GameObject teamsPanel, Text regionText)
    {
        switch (region)
        {
            case 0:
                FillTeamsPanel(americanTeams, teamsPanel, 0,6);
                regionText.text = "America";
                break;
            case 1:
                FillTeamsPanel(europeanTeams, teamsPanel, 0, 6);
                regionText.text = "Europe";
                break;
            case 2:
                FillTeamsPanel(africanTeams, teamsPanel, 0, 6);
                regionText.text = "Africa";
                break;
            case 3:
                FillTeamsPanel(asianTeams, teamsPanel, 0, 6);
                regionText.text = "Asia";
                break;
        }
    }

    void FillTeamsPanel(List<Team> teams, GameObject teamsPanel, int begin, int end)
    {
        for (int i = begin; i < end; i++)
        {
            Button newTeam = teamsPanel.transform.GetChild(i).gameObject.GetComponent<Button>();
            newTeam.onClick.AddListener(delegate { ReturnTeamSelected(newTeam.GetComponent<TeamSelected>()); });
            newTeam.GetComponent<TeamSelected>().teamInfo = teams[i];
            newTeam.image.sprite = teams[i].flag;
            newTeam.transform.GetChild(0).GetComponent<Text>().text = teams[i].teamName;
            newTeam.transform.SetParent(teamsPanel.transform);
        }
    }

    void ChangeTeamsInPanel(int bIndex, int eIndex, GameObject teamsPanel)
    {
        for (int i = 0; i < teamsPanel.transform.childCount; i++)
        {
            if (i >= bIndex && i < eIndex)
            {
                teamsPanel.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                teamsPanel.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public void ChangeTeamsIndex(string side, GameObject teamsPanel)
    {
        DeselectPreviousTeams(teamsPanel);
        if (teamsPanel.transform.childCount > 0)
        {
            if (side == "left")
            {
                if (begin - 6 < 0)
                {
                    begin = 0;
                    end = begin + 6;
                }
                else
                {
                    end = begin;
                    begin -= 6;
                }
                ChangeTeamsInPanel(begin, end, teamsPanel);
            }

            if (side == "right")
            {
                if (end + 6 > teamsPanel.transform.childCount)
                {
                    end = teamsPanel.transform.childCount;
                    begin = end - 6;
                }
                else
                {
                    begin = end;
                    end += 6;
                }
                ChangeTeamsInPanel(begin, end, teamsPanel);
            }
        }
    }

    void DeselectPreviousTeams(GameObject teamsPanel)
    {
        for (int i = 0; i < teamsPanel.transform.childCount; i++)
        {
            TeamSelected teamSelected = teamsPanel.transform.GetChild(i).GetComponent<TeamSelected>();
            if (teamSelected.isSelected)
            {
                teamSelected.isSelected = false;
                teamSelected.DeletePreviousSelected();
            }
        }
    }

    /// <summary>
    /// Return sprite with regions selected given left and right index
    /// </summary>
    /// <param name="leftIndex">Left team region index</param>
    /// <param name="rightIndex">Right team region index</param>
    /// <returns>Sprite with map region selected</returns>
    public Sprite ChangeMapSprite(int leftIndex, int rightIndex)
    {
        switch (leftIndex)
        {
            //Left team in america region
            case 0:
                switch (rightIndex)
                {
                    case 0: return mapSprites[5]; 
                    case 1: return mapSprites[8];
                    case 2: return mapSprites[6];
                    case 3: return mapSprites[7];
                }
                break;
            //Left team in europe region
            case 1:
                switch (rightIndex)
                {
                    case 0: return mapSprites[11];
                    case 1: return mapSprites[13];
                    case 2: return mapSprites[4];
                    case 3: return mapSprites[14];
                }
                break;
            //Left team in africa region
            case 2:
                switch (rightIndex)
                {
                    case 0: return mapSprites[9];
                    case 1: return mapSprites[2];
                    case 2: return mapSprites[0];
                    case 3: return mapSprites[1];
                }
                break;
            //Left team in asia region
            case 3:
                switch (rightIndex)
                {
                    case 0: return mapSprites[10];                        
                    case 1: return mapSprites[15];
                    case 2: return mapSprites[3];
                    case 3: return mapSprites[12];
                }
                break;
        }
        return mapSprites[7];
    }

    /// <summary>
    /// Get info of the team button that has been pressed and set its info to the match game object info
    /// Set default values.
    /// </summary>
    /// <param name="btnInfo"></param>
    void ReturnTeamSelected(TeamSelected btnInfo)
    {
        if (MatchInfo._matchInfo.leftTeam == null)
        {
            //Set info needed for Match scene
            MatchInfo._matchInfo.leftTeam = btnInfo.teamInfo;
            MatchInfo._matchInfo.leftTeamLineUp.defense = btnInfo.teamInfo.teamFormation.defense;
            MatchInfo._matchInfo.leftTeamLineUp.mid = btnInfo.teamInfo.teamFormation.mid;
            MatchInfo._matchInfo.leftTeamLineUp.attack = btnInfo.teamInfo.teamFormation.attack;
            MatchInfo._matchInfo.leftTeamUniform = "Local";
            //Focus on right team selection
            leftCanvas.alpha = 0.5f;
            rightCanvas.alpha = 1f;
            rightTeamsPanel.transform.GetChild(0).gameObject.GetComponent<Button>().Select();
            //Change controls to player that has right team
            QuickMatchMenuController.controller.SwitchUIControls();
        }
        else
        {
            //Set info needed for Match scene
            MatchInfo._matchInfo.rightTeam = btnInfo.teamInfo;
            MatchInfo._matchInfo.rightTeamLineUp.defense = btnInfo.teamInfo.teamFormation.defense;
            MatchInfo._matchInfo.rightTeamLineUp.mid = btnInfo.teamInfo.teamFormation.mid;
            MatchInfo._matchInfo.rightTeamLineUp.attack = btnInfo.teamInfo.teamFormation.attack;
            MatchInfo._matchInfo.rightTeamUniform = "Local";
            QuickMatchMenuController.controller.SwitchUIControls();
        }
    }
}
