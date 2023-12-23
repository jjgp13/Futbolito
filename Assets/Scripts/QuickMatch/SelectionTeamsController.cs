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
    private int leftBeginPanelIndex;

    [Header("Right Team UI")]
    public GameObject rightTeamsPanel;
    public Text rightRegionText;
    public GameObject rightTeamFlag;
    private int rightRegionIndex;
    private int rightBeginPanelIndex;

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

    private void Awake()
    {
        //Left team starting showing american region teams
        leftRegionIndex = 0;
        leftBeginPanelIndex = 0;

        //Right team starting showing asia region teams
        rightRegionIndex = 3;
        rightBeginPanelIndex = 0;

        //Load all teams and fill each region team list
        FillTeamRegions();
    }

    private void Start()
    {
        //Fill left and right panels with button flags
        FillTeamsPanel(americanTeams, leftTeamsPanel, 0);
        FillTeamsPanel(asianTeams, rightTeamsPanel, 0);
        //Select first button in left teams panel
        leftTeamsPanel.transform.GetChild(0).gameObject.GetComponent<Button>().Select();
    }

    private void Update()
    {
        //Check if selection team panel is active
        if (QuickMatchMenuController.controller.isSelectionTeamPanelActive)
        {
            if(QuickMatchMenuController.controller.leftControls.Count > 0)
            {
                //////Left team controller input
                //Left button
                if (Input.GetButtonDown(QuickMatchMenuController.controller.leftButtonString))
                {
                    if(MatchInfo._matchInfo.leftTeam == null)
                    {
                        if (leftRegionIndex == 0) leftRegionIndex = 3;
                        else leftRegionIndex--;
                        SelectedConf(leftRegionIndex, leftTeamsPanel, leftRegionText);
                        mapImage.sprite = ChangeMapSprite(leftRegionIndex, rightRegionIndex);
                    } else
                    {
                        if (rightRegionIndex == 0) rightRegionIndex = 3;
                        else rightRegionIndex--;
                        SelectedConf(rightRegionIndex, rightTeamsPanel, rightRegionText);
                        mapImage.sprite = ChangeMapSprite(leftRegionIndex, rightRegionIndex);
                    }
                }
                //Right button
                if (Input.GetButtonDown(QuickMatchMenuController.controller.rightButtonString))
                {
                    if (MatchInfo._matchInfo.leftTeam == null)
                    {
                        if (leftRegionIndex == 3) leftRegionIndex = 0;
                        else leftRegionIndex++;
                        SelectedConf(leftRegionIndex, leftTeamsPanel, leftRegionText);
                        mapImage.sprite = ChangeMapSprite(leftRegionIndex, rightRegionIndex);
                    } else {
                        if (rightRegionIndex == 3) rightRegionIndex = 0;
                        else rightRegionIndex++;
                        SelectedConf(rightRegionIndex, rightTeamsPanel, rightRegionText);
                        mapImage.sprite = ChangeMapSprite(leftRegionIndex, rightRegionIndex);
                    }
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
                FillTeamsPanel(americanTeams, teamsPanel, 0);
                regionText.text = "America";
                break;
            case 1:
                FillTeamsPanel(europeanTeams, teamsPanel, 0);
                regionText.text = "Europe";
                break;
            case 2:
                FillTeamsPanel(africanTeams, teamsPanel, 0);
                regionText.text = "Africa";
                break;
            case 3:
                FillTeamsPanel(asianTeams, teamsPanel, 0);
                regionText.text = "Asia";
                break;
        }
    }

    /// <summary>
    /// This method populate the teams panel.
    /// Since panel only shows 6 teams. it needs a index to know from which team in team list is going to show
    /// </summary>
    /// <param name="teams">Teams continent</param>
    /// <param name="teamsPanel">Left or right team panel</param>
    /// <param name="firstTeam">Index of the team list from which is start to showing</param>
    void FillTeamsPanel(List<Team> teams, GameObject teamsPanel, int firstTeam)
    {
        int teamIndex = firstTeam;
        for (int i = 0; i < 6; i++)
        {
            Button newTeam = teamsPanel.transform.GetChild(i).gameObject.GetComponent<Button>();
            //If the index is greater than the list of teams, hide those buttons on the panel
            if (teamIndex < teams.Count) {
                newTeam.gameObject.SetActive(true);
                newTeam.onClick.AddListener(delegate { ReturnTeamSelected(newTeam.GetComponent<TeamSelected>()); });
                newTeam.GetComponent<TeamSelected>().teamInfo = teams[teamIndex];
                newTeam.image.sprite = teams[teamIndex].flag;
                newTeam.transform.GetChild(0).GetComponent<Text>().text = teams[teamIndex].teamName;
                teamIndex++;
            }
            else newTeam.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// This method is attached to the "More teams" button above the panel flags.
    /// Parameter indicate for which panel is going to swtich the teams
    /// </summary>
    /// <param name="side">Left or right team panel</param>
    public void ChangeTeamsIndex(string side)
    {
        if(side == "left")
        {
            if(leftBeginPanelIndex + 6 < GetListRegionSelectedTeamsCount(leftRegionIndex).Count)
                leftBeginPanelIndex += 6;
            else
                leftBeginPanelIndex = 0;
            FillTeamsPanel(GetListRegionSelectedTeamsCount(leftRegionIndex), leftTeamsPanel, leftBeginPanelIndex);
        }
        if(side == "right")
        {
            if (rightBeginPanelIndex + 6 < GetListRegionSelectedTeamsCount(rightRegionIndex).Count)
                rightBeginPanelIndex += 6;
            else
                rightBeginPanelIndex = 0;
            FillTeamsPanel(GetListRegionSelectedTeamsCount(rightRegionIndex), rightTeamsPanel, rightBeginPanelIndex);
        }
    }


    /// <summary>
    /// Return the list of the teams given the index of each side
    /// </summary>
    /// <param name="region">Index of the list (0: America, 1:Europe, 2:Africa, 3:Asia)</param>
    /// <returns>List with teams of that continent</returns>
    private List<Team> GetListRegionSelectedTeamsCount(int region)
    {
        switch (region)
        {
            case 0: return americanTeams;
            case 1: return europeanTeams;
            case 2: return africanTeams;
            case 3: return asianTeams;
        }
        return americanTeams;
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
