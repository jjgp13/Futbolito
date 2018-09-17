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

    public Button buttonLeft, buttonRight;

    public GameObject playerInfo;
    public GameObject comInfo;

    public Button setMatchBtn;


    public void SelectedConf(string conf)
    {
        switch (conf)
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
            default:
                break;
        }
    }

    void FillTeamsPanel(Team[] teams)
    {
        DeleteTeamsFromPanel();
        for (int i = 0; i < teams.Length; i++)
        {
            Button newTeam = Instantiate(teamButton);
            newTeam.image.sprite = teams[i].flag;
            newTeam.transform.SetParent(teamsPanel.transform);
        }
    }

    void DeleteTeamsFromPanel()
    {
        foreach (Transform child in teamsPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

}
