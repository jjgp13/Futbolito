using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickMatchMenuController : MonoBehaviour {


    //Reference to panel that handles teams
    public GameObject teamsPanel;
    public Button teamButton;

    //Reference to continent buttons and data for teams.
    public Button[] confBtns;
    public Team[] americanTeams;
    public Team[] europeTeams;
    public Team[] africanTeams;
    public Team[] asianTeams;

    public Image mapRegion;
    public Text teamsRegion;

    public Button buttonLeft, buttonRight;
    int begin, end;

    public GameObject playerInfo;
    public GameObject comInfo;


    public GameObject matchInfo;
    public Button setMatchBtn;


    public void SelectedConf(string region)
    {
        teamsRegion.text = region;
        switch (region)
        {
            case "America":
                FillTeamsPanel(americanTeams);
                break;
            case "Europe":
                FillTeamsPanel(europeTeams);
                break;
            case "Africa":
                FillTeamsPanel(africanTeams);
                break;
            case "Asia":
                FillTeamsPanel(asianTeams);
                break;
        }
    }

    void FillTeamsPanel(Team[] teams)
    {
        begin = 0;
        end = 6;
        DeleteTeamsFromPanel();
        for (int i = 0; i < teams.Length; i++)
        {
            Button newTeam = Instantiate(teamButton);
            newTeam.onClick.AddListener(delegate { ReturnTeamSelected(newTeam.GetComponent<TeamSelected>()); });
            newTeam.GetComponent<TeamSelected>().team = teams[i];
            newTeam.image.sprite = teams[i].flag;
            newTeam.transform.GetChild(0).GetComponent<Text>().text = teams[i].teamName;
            newTeam.transform.SetParent(teamsPanel.transform);
            if(i >= 0 && i < 6) newTeam.gameObject.SetActive(true);
            else newTeam.gameObject.SetActive(false);
        }
    }

    void DeleteTeamsFromPanel()
    {
        foreach (Transform child in teamsPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void ChangeTeamsInPanel(int bIndex, int eIndex)
    {
        for (int i = 0; i < teamsPanel.transform.childCount; i++)
        {
            if (i >= bIndex && i < eIndex)
            {
                teamsPanel.transform.GetChild(i).gameObject.SetActive(true);
            } else
            {
                teamsPanel.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public void ChangeTeamsIndex(string side)
    {
        DeselectPreviousTeams();
        if(teamsPanel.transform.childCount > 0)
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
                ChangeTeamsInPanel(begin, end);
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
                ChangeTeamsInPanel(begin, end);
            }
        }
    }

    void DeselectPreviousTeams()
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

    public void ChangeMapSprite(Sprite mapSprite)
    {
        mapRegion.sprite = mapSprite;
    }

    void ReturnTeamSelected(TeamSelected btnInfo)
    {
        MatchInfo info = matchInfo.GetComponent<MatchInfo>();
        if (info.playerTeam == null)
        {
            info.playerTeam = btnInfo.team;
            info.SetFlags("PlayerFlags", btnInfo.team.flag, btnInfo.team.teamName);
        }
        else
        {
            info.comTeam = btnInfo.team;
            info.SetFlags("ComFlags", btnInfo.team.flag, btnInfo.team.teamName);
        }
    }

}
