﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickMatchMenuController : MonoBehaviour {

    //This game ob
    public GameObject matchInfo;

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

    public Button setMatchBtn;
    public GameObject matchSettingMenu;
    public Image playerFormationImage, comFormationImage;
    


    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(matchInfo);
    }

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

    /// <summary>
    /// Get info of the team button that has been pressed and set its info to the match game object info
    /// Set default values.
    /// </summary>
    /// <param name="btnInfo"></param>
    void ReturnTeamSelected(TeamSelected btnInfo)
    {
        if (MatchInfo._matchInfo.playerTeam == null)
        {
            //Set info needed for Match scene
            MatchInfo._matchInfo.playerTeam = btnInfo.team;
            
            MatchInfo._matchInfo.playerLineUp.defense = btnInfo.team.teamFormation.defense;
            MatchInfo._matchInfo.playerLineUp.mid = btnInfo.team.teamFormation.mid;
            MatchInfo._matchInfo.playerLineUp.attack = btnInfo.team.teamFormation.attack;
            MatchInfo._matchInfo.playerUniform = "Local";

            //Set UI given team selected
            SetFlags("PlayerFlags", btnInfo.team.flag, btnInfo.team.teamName);
            playerFormationImage.sprite = btnInfo.team.formationImage;
        }
        else
        {
            //Set info needed for Match scene
            MatchInfo._matchInfo.comTeam = btnInfo.team;
            MatchInfo._matchInfo.comLineUp.defense = btnInfo.team.teamFormation.defense;
            MatchInfo._matchInfo.comLineUp.mid = btnInfo.team.teamFormation.mid;
            MatchInfo._matchInfo.comLineUp.attack = btnInfo.team.teamFormation.attack;
            MatchInfo._matchInfo.comUniform = "Local";

            //Set UI given team selected
            SetFlags("ComFlags", btnInfo.team.flag, btnInfo.team.teamName);
            comFormationImage.sprite = btnInfo.team.formationImage;
        }
    }

    //On click MatchSettings button it will show the match settings panel
    public void MatchSettingMenuAnimation(bool state)
    {
        Animator anim = matchSettingMenu.GetComponent<Animator>();
        anim.SetBool("Show", state);
    }

    //On click Team button this will set the UI flags in main panel and match settings panel
    void SetFlags(string tag, Sprite flag, string teamName)
    {
        GameObject[] flags = GameObject.FindGameObjectsWithTag(tag);
        foreach (var item in flags)
        {
            item.GetComponent<Image>().sprite = flag;
            item.transform.GetChild(1).GetComponent<Text>().text = teamName;
        }
    }

    //Set uniforms buttons for a team
    //void SetUniforms(string tag)
}
