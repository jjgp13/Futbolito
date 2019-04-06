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
    public Text leftRegionSelected;
    public GameObject leftTeamFlag;
    private bool isLeftTeamSelected;
    private int leftRegionIndex;

    [Header("Right Team UI")]
    public GameObject rightTeamsPanel;
    public Text rightRegionSelected;
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
        //Fill left and right panels with button flags
        FillTeamsPanel(americanTeams, leftTeamsPanel);
        FillTeamsPanel(asianTeams, rightTeamsPanel);
    }

    private void Update()
    {
        if (QuickMatchMenuController.controller.selectionTeamPanel)
        {
            if (LeftButtonPressed(QuickMatchMenuController.controller.leftControls))
            {
                if (leftRegionIndex == 0)
                {
                    leftRegionIndex = 3;
                    SelectedConf(leftRegionIndex, leftTeamsPanel);
                }
                else
                {
                    leftRegionIndex--;
                    SelectedConf(leftRegionIndex, leftTeamsPanel);
                }
            }
        }
        
    }

    private bool LeftButtonPressed( List<ControlMapping> teamControl)
    {
        if (Input.GetButton(teamControl[0].leftButton) || Input.GetButton(teamControl[1].leftButton))
            return true;
        else
            return false;
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

    public void SelectedConf(int region, GameObject teamsPanel)
    {
        //teamsRegion.text = region;
        switch (region)
        {
            case 0:
                FillTeamsPanel(americanTeams, teamsPanel);
                break;
            case 1:
                FillTeamsPanel(europeanTeams, teamsPanel);
                break;
            case 2:
                FillTeamsPanel(africanTeams, teamsPanel);
                break;
            case 3:
                FillTeamsPanel(asianTeams, teamsPanel);
                break;
        }
    }

    void FillTeamsPanel(List<Team> teams, GameObject teamsPanel)
    {
        begin = 0;
        end = teams.Count;
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
        }
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

    public void ChangeMapSprite(Sprite mapSprite, int leftIndex, int rightIndex)
    {
        switch (leftIndex)
        {
            //Left team in america region
            case 0:
                if(rightIndex == 0)
                {

                }
                break;
            //Left team in europe region
            case 1:
                break;
            //Left team in africa region
            case 2:
                break;
            //Left team in europe region
            case 3:
                break;
        }
        if (leftIndex == 0 || rightIndex == 0) 
        mapImage.sprite = mapSprite;
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
