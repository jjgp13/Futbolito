using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectionTeamsController : MonoBehaviour
{
    [Header("Teams flag button prefab")]
    public Button teamButton;

    [Header("Left Team UI")]
    public GameObject leftTeamsPanel;
    public EventSystem leftTeamSystem;
    public StandaloneInputModule leftInputModule;
    
    public Text leftRegionText;
    public GameObject leftTeamFlag;
    private bool isLeftTeamSelected;
    private int leftRegionIndex;

    [Header("Right Team UI")]
    public GameObject rightTeamsPanel;
    public EventSystem rightTeamSystem;
    public StandaloneInputModule rightInputModule;
    public Text rightRegionText;
    public GameObject rightTeamFlag;
    private bool isRightTeamSelected;
    private int rightRegionIndex;

    [Header("Map image")]
    public Image mapImage;
    public Sprite[] mapSprites;

    //America: 0, Europe: 1, Africa:2, Asia:4
    private List<Team> americanTeams = new List<Team>();
    private List<Team> europeanTeams = new List<Team>();
    private List<Team> africanTeams = new List<Team>();
    private List<Team> asianTeams = new List<Team>();

    int begin, end;

    public GameObject clearTeamSelectionButton;
    public Sprite flagOutline;

    private void Awake()
    {
        
        //None of the teams have been selected when panel is presenting
        isLeftTeamSelected = false;
        isRightTeamSelected = false;
        //Left team starting showing american region teams
        leftRegionIndex = 0;
        //Right team starting showing asia region teams
        rightRegionIndex = 3;
        //Load all teams and fill each region team list
        FillTeamRegions();
    }

    private void Start()
    {
        leftTeamsPanel.transform.GetChild(0).gameObject.GetComponent<Button>().Select();
        //Fill left and right panels with button flags
        //FillTeamsPanel(americanTeams, leftTeamsPanel);
        //FillTeamsPanel(asianTeams, rightTeamsPanel);
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

    public void SelectedConf(int region, GameObject teamsPanel, Text regionText)
    {
        //teamsRegion.text = region;
        switch (region)
        {
            case 0:
                FillTeamsPanel(americanTeams, teamsPanel);
                regionText.text = "America";
                break;
            case 1:
                FillTeamsPanel(europeanTeams, teamsPanel);
                regionText.text = "Europe";
                break;
            case 2:
                FillTeamsPanel(africanTeams, teamsPanel);
                regionText.text = "Africa";
                break;
            case 3:
                FillTeamsPanel(asianTeams, teamsPanel);
                regionText.text = "Asia";
                break;
        }
    }

    void FillTeamsPanel(List<Team> teams, GameObject teamsPanel)
    {
        begin = 0;
        end = 6;
        DeleteTeamsFromPanel(teamsPanel);
        for (int i = 0; i < teams.Count; i++)
        {
            Button newTeam = Instantiate(teamButton);
            newTeam.onClick.AddListener(delegate { ReturnTeamSelected(newTeam.GetComponent<TeamSelected>()); });
            newTeam.GetComponent<TeamSelected>().team = teams[i];
            newTeam.image.sprite = teams[i].flag;
            newTeam.transform.GetChild(0).GetComponent<Text>().text = teams[i].teamName;
            newTeam.transform.SetParent(teamsPanel.transform);
            if (i >= begin && i < end) newTeam.gameObject.SetActive(true);
            else newTeam.gameObject.SetActive(false);
        }
        teamsPanel.transform.GetChild(0).GetComponent<Button>().Select();
    }
    

    void DeleteTeamsFromPanel(GameObject teamsPanel)
    {
        foreach (Transform child in teamsPanel.transform)
        {
            Destroy(child.gameObject);
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
            MatchInfo._matchInfo.leftTeam = btnInfo.team;

            MatchInfo._matchInfo.leftTeamLineUp.defense = btnInfo.team.teamFormation.defense;
            MatchInfo._matchInfo.leftTeamLineUp.mid = btnInfo.team.teamFormation.mid;
            MatchInfo._matchInfo.leftTeamLineUp.attack = btnInfo.team.teamFormation.attack;
            MatchInfo._matchInfo.leftTeamUniform = "Local";

        }
        else
        {
            //Set info needed for Match scene
            MatchInfo._matchInfo.rightTeam = btnInfo.team;
            MatchInfo._matchInfo.rightTeamLineUp.defense = btnInfo.team.teamFormation.defense;
            MatchInfo._matchInfo.rightTeamLineUp.mid = btnInfo.team.teamFormation.mid;
            MatchInfo._matchInfo.rightTeamLineUp.attack = btnInfo.team.teamFormation.attack;
            MatchInfo._matchInfo.rightTeamUniform = "Local";
        }
    }
}
